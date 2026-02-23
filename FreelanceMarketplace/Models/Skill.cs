using System.ComponentModel.DataAnnotations;

namespace FreelanceMarketplace.Models;

/// <summary>Platform skill (e.g. React, Figma, Flutter). Used by freelancers and job listings.</summary>
public class Skill
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }

    public ICollection<FreelancerSkill> FreelancerSkills { get; set; } = new List<FreelancerSkill>();

    public ICollection<JobListingSkill> JobListingSkills { get; set; } = new List<JobListingSkill>();
}
