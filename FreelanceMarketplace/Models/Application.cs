using System.ComponentModel.DataAnnotations;

namespace FreelanceMarketplace.Models;

/// <summary>Freelancer's application to a job listing. One application per freelancer per listing.</summary>
public class Application
{
    public int Id { get; set; }

    public int JobListingId { get; set; }

    public JobListing JobListing { get; set; } = null!;

    public int FreelancerId { get; set; }

    public FreelancerProfile Freelancer { get; set; } = null!;

    [Required]
    [MaxLength(2000)]
    public required string CoverMessage { get; set; }

    [Required]
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Conversation? Conversation { get; set; }
}
