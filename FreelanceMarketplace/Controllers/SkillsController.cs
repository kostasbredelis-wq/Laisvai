using FreelanceMarketplace.Data;
using FreelanceMarketplace.DTOs;
using FreelanceMarketplace.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FreelanceMarketplace.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SkillsController : ControllerBase
{
    private readonly AppDbContext _context;

    public SkillsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<SkillResponseDto>>> GetSkills(CancellationToken cancellationToken)
    {
        var skills = await _context.Skills
            .OrderBy(s => s.Name)
            .Select(s => new SkillResponseDto
            {
                Id = s.Id,
                Name = s.Name
            })
            .ToListAsync(cancellationToken);

        return Ok(skills);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SkillResponseDto>> CreateSkill(
        CreateSkillDto dto,
        CancellationToken cancellationToken)
    {
        var normalizedName = dto.Name.Trim();

        var exists = await _context.Skills
            .AnyAsync(s => s.Name.ToLower() == normalizedName.ToLower(), cancellationToken);

        if (exists)
        {
            return BadRequest(new { message = "Skill with this name already exists." });
        }

        var skill = new Skill
        {
            Name = normalizedName
        };

        _context.Skills.Add(skill);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new SkillResponseDto
        {
            Id = skill.Id,
            Name = skill.Name
        };

        return CreatedAtAction(nameof(GetSkills), null, response);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteSkill(int id, CancellationToken cancellationToken)
    {
        var skill = await _context.Skills
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (skill == null)
        {
            return NotFound();
        }

        var inUse = await _context.FreelancerSkills
            .AnyAsync(fs => fs.SkillId == id, cancellationToken)
            || await _context.JobListingSkills
            .AnyAsync(js => js.SkillId == id, cancellationToken);

        if (inUse)
        {
            return BadRequest(new { message = "Cannot delete a skill that is currently in use." });
        }

        _context.Skills.Remove(skill);
        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}

