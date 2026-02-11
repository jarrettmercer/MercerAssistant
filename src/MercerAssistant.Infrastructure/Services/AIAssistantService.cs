using System.Text.Json;
using MercerAssistant.Core.DTOs;
using MercerAssistant.Core.Interfaces;
using MercerAssistant.Infrastructure.AI;
using MercerAssistant.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;

using DbChatMessage = MercerAssistant.Core.Entities.ChatMessage;

namespace MercerAssistant.Infrastructure.Services;

public class AIAssistantService : IAIAssistantService
{
    private readonly AppDbContext _db;
    private readonly ISchedulingService _scheduling;
    private readonly ILogger<AIAssistantService> _logger;
    private readonly ChatClient? _chatClient;

    private const int MaxToolIterations = 10;
    private const int MaxConversationHistory = 50;

    private static readonly ChatTool CheckAvailabilityTool = ChatTool.CreateFunctionTool(
        functionName: "check_availability",
        functionDescription: "Check available time slots for a specific date",
        functionParameters: BinaryData.FromString("""
        {
            "type": "object",
            "properties": {
                "date": { "type": "string", "description": "The date in yyyy-MM-dd format" },
                "durationMinutes": { "type": "integer", "description": "Duration in minutes (default 30)" }
            },
            "required": ["date"]
        }
        """));

    private static readonly ChatTool CreateBookingTool = ChatTool.CreateFunctionTool(
        functionName: "create_booking",
        functionDescription: "Create a new booking/appointment",
        functionParameters: BinaryData.FromString("""
        {
            "type": "object",
            "properties": {
                "clientName": { "type": "string", "description": "Client's full name" },
                "clientEmail": { "type": "string", "description": "Client's email address" },
                "startTime": { "type": "string", "description": "Start time in ISO 8601 format (yyyy-MM-ddTHH:mm:ss)" },
                "durationMinutes": { "type": "integer", "description": "Duration in minutes (default 30)" },
                "notes": { "type": "string", "description": "Optional notes about the appointment" }
            },
            "required": ["clientName", "clientEmail", "startTime"]
        }
        """));

    private static readonly ChatTool CancelBookingTool = ChatTool.CreateFunctionTool(
        functionName: "cancel_booking",
        functionDescription: "Cancel an existing appointment by its ID",
        functionParameters: BinaryData.FromString("""
        {
            "type": "object",
            "properties": {
                "appointmentId": { "type": "string", "description": "The appointment ID (GUID)" },
                "reason": { "type": "string", "description": "Optional reason for cancellation" }
            },
            "required": ["appointmentId"]
        }
        """));

    private static readonly ChatTool ListUpcomingTool = ChatTool.CreateFunctionTool(
        functionName: "list_upcoming_appointments",
        functionDescription: "List upcoming appointments in chronological order",
        functionParameters: BinaryData.FromString("""
        {
            "type": "object",
            "properties": {
                "count": { "type": "integer", "description": "Number of appointments to return (default 5)" }
            },
            "required": []
        }
        """));

    public AIAssistantService(
        AppDbContext db,
        ISchedulingService scheduling,
        ILogger<AIAssistantService> logger,
        IServiceProvider serviceProvider)
    {
        _db = db;
        _scheduling = scheduling;
        _logger = logger;
        _chatClient = serviceProvider.GetService<ChatClient>();
    }

    public async Task<Guid> StartConversationAsync(string userId)
    {
        var conversation = new MercerAssistant.Core.Entities.ChatConversation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _db.ChatConversations.Add(conversation);
        await _db.SaveChangesAsync();

        return conversation.Id;
    }

    public async Task<ChatResponseDto> SendMessageAsync(Guid conversationId, string userMessage)
    {
        if (_chatClient is null)
        {
            return new ChatResponseDto(
                "AI is not configured. Please set the Azure AI endpoint and API key in appsettings.json.",
                false, null);
        }

        var conversation = await _db.ChatConversations
            .FirstOrDefaultAsync(c => c.Id == conversationId);

        if (conversation is null)
            return new ChatResponseDto("Conversation not found.", false, null);

        // Save user message to DB
        _db.ChatMessages.Add(new DbChatMessage
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            Role = "user",
            Content = userMessage,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        // Load recent message history (includes the just-saved user message)
        var recentMessages = await _db.ChatMessages
            .Where(m => m.ConversationId == conversationId)
            .OrderByDescending(m => m.CreatedAt)
            .Take(MaxConversationHistory)
            .ToListAsync();
        recentMessages.Reverse();

        // Build OpenAI message list
        var messages = new List<ChatMessage>();

        messages.Add(new SystemChatMessage(
            SystemPrompts.SchedulingAssistant +
            $"\n\nCurrent date/time: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC"));

        foreach (var msg in recentMessages)
        {
            if (msg.Role == "user")
                messages.Add(new UserChatMessage(msg.Content));
            else if (msg.Role == "assistant")
                messages.Add(new AssistantChatMessage(msg.Content));
        }

        // Configure tools
        var options = new ChatCompletionOptions();
        options.Tools.Add(CheckAvailabilityTool);
        options.Tools.Add(CreateBookingTool);
        options.Tools.Add(CancelBookingTool);
        options.Tools.Add(ListUpcomingTool);

        var tools = new SchedulingTools(_scheduling, conversation.UserId);
        string? lastToolName = null;
        bool toolWasInvoked = false;

        try
        {
            // Tool call loop
            for (int i = 0; i < MaxToolIterations; i++)
            {
                ChatCompletion completion = await _chatClient.CompleteChatAsync(messages, options);

                if (completion.FinishReason == ChatFinishReason.ToolCalls)
                {
                    messages.Add(new AssistantChatMessage(completion));

                    foreach (var toolCall in completion.ToolCalls)
                    {
                        _logger.LogInformation("Tool call: {Tool} with args: {Args}",
                            toolCall.FunctionName, toolCall.FunctionArguments.ToString());

                        var result = await DispatchToolCallAsync(tools, toolCall);
                        messages.Add(new ToolChatMessage(toolCall.Id, result));

                        lastToolName = toolCall.FunctionName;
                        toolWasInvoked = true;
                    }
                }
                else
                {
                    var responseText = completion.Content.Count > 0
                        ? completion.Content[0].Text
                        : "I'm sorry, I couldn't generate a response.";

                    // Save assistant response to DB
                    _db.ChatMessages.Add(new DbChatMessage
                    {
                        Id = Guid.NewGuid(),
                        ConversationId = conversationId,
                        Role = "assistant",
                        Content = responseText,
                        ToolCallName = lastToolName,
                        CreatedAt = DateTime.UtcNow
                    });

                    conversation.LastMessageAt = DateTime.UtcNow;
                    await _db.SaveChangesAsync();

                    return new ChatResponseDto(responseText, toolWasInvoked, lastToolName);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Azure OpenAI API call failed for conversation {Id}", conversationId);
            return new ChatResponseDto(
                "I'm sorry, I encountered an error communicating with the AI service. Please try again.",
                false, null);
        }

        return new ChatResponseDto(
            "I'm sorry, I encountered an issue processing your request. Please try again.",
            toolWasInvoked, lastToolName);
    }

    public async IAsyncEnumerable<string> StreamMessageAsync(Guid conversationId, string userMessage)
    {
        yield return "AI streaming will be connected in Phase 7.";
        await Task.CompletedTask;
    }

    private async Task<string> DispatchToolCallAsync(SchedulingTools tools, ChatToolCall toolCall)
    {
        try
        {
            using var args = JsonDocument.Parse(toolCall.FunctionArguments.ToString());
            var root = args.RootElement;

            return toolCall.FunctionName switch
            {
                "check_availability" => await tools.CheckAvailability(
                    root.GetProperty("date").GetString()!,
                    root.TryGetProperty("durationMinutes", out var d) ? d.GetInt32() : 30),

                "create_booking" => await tools.CreateBooking(
                    root.GetProperty("clientName").GetString()!,
                    root.GetProperty("clientEmail").GetString()!,
                    root.GetProperty("startTime").GetString()!,
                    root.TryGetProperty("durationMinutes", out var dm) ? dm.GetInt32() : 30,
                    root.TryGetProperty("notes", out var n) ? n.GetString() : null),

                "cancel_booking" => await tools.CancelBooking(
                    root.GetProperty("appointmentId").GetString()!,
                    root.TryGetProperty("reason", out var r) ? r.GetString() : null),

                "list_upcoming_appointments" => await tools.ListUpcomingAppointments(
                    root.TryGetProperty("count", out var c) ? c.GetInt32() : 5),

                _ => $"Unknown tool: {toolCall.FunctionName}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to dispatch tool call {Tool}", toolCall.FunctionName);
            return $"Error executing {toolCall.FunctionName}: {ex.Message}";
        }
    }
}
