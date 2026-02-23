using System.ComponentModel.DataAnnotations;

namespace FreelanceMarketplace.Models;

/// <summary>Review left by a client on a freelancer. One review per client per freelancer.</summary>
public class Review
{
    public int Id { get; set; }

    public int ClientId { get; set; }

    public ClientProfile Client { get; set; } = null!;

    public int FreelancerId { get; set; }

    public FreelancerProfile Freelancer { get; set; } = null!;

    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }

    [Required]
    [MaxLength(2000)]
    public required string Comment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
