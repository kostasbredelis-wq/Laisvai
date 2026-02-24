using System.ComponentModel.DataAnnotations;
using FreelanceMarketplace.Models;

namespace FreelanceMarketplace.DTOs;

public class CreateServiceDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(4000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public FreelancerCategory Category { get; set; }

    [Required]
    public PricingType PricingType { get; set; }
}

public class UpdateServiceDto
{
    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(4000)]
    public string? Description { get; set; }

    public FreelancerCategory? Category { get; set; }

    public PricingType? PricingType { get; set; }
}

public class ServiceResponseDto
{
    public int Id { get; set; }
    public int FreelancerId { get; set; }
    public string FreelancerName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string PricingType { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
