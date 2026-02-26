using System.ComponentModel.DataAnnotations;

namespace FreelanceMarketplace.DTOs;

public class CreateReviewDto
{
    [Required]
    public int FreelancerId { get; set; }

    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }

    [Required]
    [MaxLength(2000)]
    public required string Comment { get; set; }
}

public class ReviewResponseDto
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public int FreelancerId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
