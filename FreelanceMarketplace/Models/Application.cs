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
    public ApplicationStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
}
