using MercerAssistant.Core.DTOs;
using MercerAssistant.Core.Entities;
using MercerAssistant.Core.Interfaces;
using MercerAssistant.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace MercerAssistant.Infrastructure.Services;

/// <summary>
/// Stub AI assistant service. Full GPT 5.1 mini integration added in Phase 3.
/// </summary>
public class AIAssistantService : IAIAssistantService
{
    private readonly AppDbContext _db;
    private readonly ILogger<AIAssistantService> _logger;

    public AIAssistantService(AppDbContext db, ILogger<AIAssistantService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<Guid> StartConversationAsync(string userId)
    {
        var conversation = new ChatConversation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _db.ChatConversations.Add(conversation);
        await _db.SaveChangesAsync();

        return conversation.Id;
    }

    public Task<ChatResponseDto> SendMessageAsync(Guid conversationId, string userMessage)
    {
        _logger.LogInformation("[Stub] AI chat message in conversation {Id}: {Message}",
            conversationId, userMessage);

        return Task.FromResult(new ChatResponseDto(
            "AI chat will be connected in Phase 3. Your message was: " + userMessage,
            false,
            null));
    }

    public async IAsyncEnumerable<string> StreamMessageAsync(Guid conversationId, string userMessage)
    {
        yield return "AI streaming will be connected in Phase 7.";
        await Task.CompletedTask;
    }
}
