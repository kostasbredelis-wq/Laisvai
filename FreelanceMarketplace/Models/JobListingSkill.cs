namespace FreelanceMarketplace.Models;

/// <summary>Junction table linking job listings to required skills. Composite PK (JobListingId, SkillId).</summary>
public class JobListingSkill
{
    public int JobListingId { get; set; }

    public JobListing JobListing { get; set; } = null!;

    public int SkillId { get; set; }

    public Skill Skill { get; set; } = null!;
}
