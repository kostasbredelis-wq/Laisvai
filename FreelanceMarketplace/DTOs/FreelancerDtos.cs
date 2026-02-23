using System.ComponentModel.DataAnnotations;
using FreelanceMarketplace.Models;

namespace FreelanceMarketplace.DTOs;

public class CreateFreelancerProfileDto
{
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
}

public class UpdateFreelancerProfileDto
{
    [MaxLength(200)]
    public string? FullName { get; set; }

    [MaxLength(2000)]
    public string? Bio { get; set; }

    public FreelancerCategory? Category { get; set; }

    [MaxLength(500)]
    public string? PortfolioUrl { get; set; }

    [MaxLength(500)]
    public string? AvatarUrl { get; set; }
}

public class FreelancerProfileResponseDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public required string FullName { get; set; }
    public string? Bio { get; set; }
    public required string Category { get; set; }
    public string? PortfolioUrl { get; set; }
    public string? AvatarUrl { get; set; }
    public required string ApplicationStatus { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string> Skills { get; set; } = new();
}
