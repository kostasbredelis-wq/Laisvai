using System.ComponentModel.DataAnnotations;

namespace FreelanceMarketplace.Models;

/// <summary>Service offering posted by a freelancer.</summary>
public class Service
{
    public int Id { get; set; }

    public int FreelancerId { get; set; }

    public FreelancerProfile Freelancer { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public required string Title { get; set; }

    [Required]
    [MaxLength(4000)]
    public required string Description { get; set; }

    [Required]
    public FreelancerCategory Category { get; set; }

    [Required]
    public PricingType PricingType { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
