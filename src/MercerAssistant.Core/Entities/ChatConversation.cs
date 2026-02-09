namespace MercerAssistant.Core.Entities;

public class ChatConversation
{
    public Guid Id { get; set; }

    public string UserId { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;

    public string? Title { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastMessageAt { get; set; }

    public ICollection<ChatMessage> Messages { get; set; } = [];
}
