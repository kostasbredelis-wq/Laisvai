using System.ComponentModel.DataAnnotations;

namespace FreelanceMarketplace.Models;

/// <summary>Message within a conversation between freelancer and client.</summary>
public class Message
{
    public int Id { get; set; }

    public int ConversationId { get; set; }

    public Conversation Conversation { get; set; } = null!;

    public int SenderId { get; set; }

    public User Sender { get; set; } = null!;

    [Required]
    [MaxLength(4000)]
    public required string Content { get; set; }

    public bool IsRead { get; set; } = false;

    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
