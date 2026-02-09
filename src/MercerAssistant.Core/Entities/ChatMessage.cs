using System.ComponentModel.DataAnnotations;

namespace MercerAssistant.Core.Entities;

public class ChatMessage
{
    public Guid Id { get; set; }

    public Guid ConversationId { get; set; }
    public ChatConversation Conversation { get; set; } = null!;

    [Required, StringLength(20)]
    public string Role { get; set; } = "user";

    [Required]
    public string Content { get; set; } = "";

    public string? ToolCallName { get; set; }

    public int? TokensUsed { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
