using System.ComponentModel.DataAnnotations;

namespace FreelanceMarketplace.DTOs;

public class SkillResponseDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
}

public class CreateSkillDto
{
    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }
}

