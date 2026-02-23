using System.ComponentModel.DataAnnotations;

namespace FreelanceMarketplace.Models;

/// <summary>Freelancer profile linked to a user. Contains display info and application status.</summary>
public class FreelancerProfile
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public User User { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public required string FullName { get; set; }

    [MaxLength(2000)]
    public string? Bio { get; set; }

    [Required]
    public FreelancerCategory Category { get; set; }

    [MaxLength(500)]
    public string? PortfolioUrl { get; set; }

    [MaxLength(500)]
    public string? AvatarUrl { get; set; }

    [Required]
    public FreelancerApplicationStatus ApplicationStatus { get; set; } = FreelancerApplicationStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<FreelancerSkill> FreelancerSkills { get; set; } = new List<FreelancerSkill>();

    public ICollection<Service> Services { get; set; } = new List<Service>();

    public ICollection<Application> Applications { get; set; } = new List<Application>();

    public ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();

    public ICollection<Review> ReviewsReceived { get; set; } = new List<Review>();
}
