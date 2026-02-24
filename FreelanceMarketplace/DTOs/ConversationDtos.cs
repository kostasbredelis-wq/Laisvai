using System.ComponentModel.DataAnnotations;

namespace FreelanceMarketplace.DTOs;

public class MessageResponseDto
{
    public int Id { get; set; }
    public int ConversationId { get; set; }
    public int SenderId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime SentAt { get; set; }
}

public class ConversationSummaryDto
{
    public int Id { get; set; }
    public int FreelancerId { get; set; }
    public string FreelancerName { get; set; } = string.Empty;
    public int ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public int? JobListingId { get; set; }
    public string? JobTitle { get; set; }
    public DateTime LastMessageAt { get; set; }
    public int UnreadCount { get; set; }
}

public class ConversationDetailDto : ConversationSummaryDto
{
    public List<MessageResponseDto> Messages { get; set; } = [];
}

public class SendMessageDto
{
    [Required]
    [MaxLength(4000)]
    public required string Content { get; set; }
}
