using System.ComponentModel.DataAnnotations;

namespace FreelanceMarketplace.DTOs;

public class CreateApplicationDto
{
    [Required]
    [MaxLength(2000)]
    public required string CoverMessage { get; set; }
}

public class ApplicationResponseDto
{
    public int Id { get; set; }
    public int JobListingId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public int FreelancerId { get; set; }
    public string FreelancerName { get; set; } = string.Empty;
    public string CoverMessage { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
