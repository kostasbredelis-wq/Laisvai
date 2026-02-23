using System.ComponentModel.DataAnnotations;

namespace FreelanceMarketplace.Models;

/// <summary>Client profile linked to a user. Company or individual hiring freelancers.</summary>
public class ClientProfile
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public User User { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public required string DisplayName { get; set; }

    [MaxLength(2000)]
    public string? About { get; set; }

    [MaxLength(500)]
    public string? Website { get; set; }

    [MaxLength(500)]
    public string? AvatarUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<JobListing> JobListings { get; set; } = new List<JobListing>();

    public ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();

    public ICollection<Review> ReviewsLeft { get; set; } = new List<Review>();
}
