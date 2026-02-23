using System.ComponentModel.DataAnnotations;

namespace FreelanceMarketplace.Models;

/// <summary>Job listing posted by a client.</summary>
public class JobListing
{
    public int Id { get; set; }

    public int ClientId { get; set; }

    public ClientProfile Client { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public required string Title { get; set; }

    [Required]
    [MaxLength(4000)]
    public required string Description { get; set; }

    [Required]
    public FreelancerCategory Category { get; set; }

    [Required]
    public BudgetType BudgetType { get; set; }

    public DateTime? Deadline { get; set; }

    public bool IsOpen { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<JobListingSkill> JobListingSkills { get; set; } = new List<JobListingSkill>();

    public ICollection<Application> Applications { get; set; } = new List<Application>();

    public ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
}
