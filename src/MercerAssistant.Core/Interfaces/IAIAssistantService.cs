using MercerAssistant.Core.DTOs;

namespace MercerAssistant.Core.Interfaces;

public interface IAIAssistantService
{
    Task<ChatResponseDto> SendMessageAsync(Guid conversationId, string userMessage);
    Task<Guid> StartConversationAsync(string userId);
    IAsyncEnumerable<string> StreamMessageAsync(Guid conversationId, string userMessage);
}
