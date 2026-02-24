using System.ComponentModel.DataAnnotations;
using FreelanceMarketplace.Models;

namespace FreelanceMarketplace.DTOs;

public class CreateJobListingDto
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
    public BudgetType BudgetType { get; set; }

    public DateTime? Deadline { get; set; }

    public List<int>? SkillIds { get; set; }
}

public class UpdateJobListingDto
{
    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(4000)]
    public string? Description { get; set; }

    public FreelancerCategory? Category { get; set; }

    public BudgetType? BudgetType { get; set; }

    public DateTime? Deadline { get; set; }

    public List<int>? SkillIds { get; set; }
}

public class JobListingResponseDto
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string BudgetType { get; set; } = string.Empty;
    public DateTime? Deadline { get; set; }
    public bool IsOpen { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string> Skills { get; set; } = [];
}
