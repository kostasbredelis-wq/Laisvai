namespace FreelanceMarketplace.Models;

/// <summary>Junction table linking freelancers to their skills. Composite PK (FreelancerId, SkillId).</summary>
public class FreelancerSkill
{
    public int FreelancerId { get; set; }

    public FreelancerProfile Freelancer { get; set; } = null!;

    public int SkillId { get; set; }

    public Skill Skill { get; set; } = null!;
}
