using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FreelanceMarketplace.Data;
using FreelanceMarketplace.DTOs;
using FreelanceMarketplace.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FreelanceMarketplace.Controllers;

[Route("api/freelancers/me/skills")]
[ApiController]
public class FreelancerSkillsController : ControllerBase
{
    private readonly AppDbContext _context;

    public FreelancerSkillsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize(Roles = "Freelancer")]
    public async Task<ActionResult<IEnumerable<SkillResponseDto>>> GetMySkills(CancellationToken cancellationToken)
    {
        var userId = GetUserId();

        var freelancer = await _context.FreelancerProfiles
            .Include(fp => fp.FreelancerSkills)
                .ThenInclude(fs => fs.Skill)
            .FirstOrDefaultAsync(fp => fp.UserId == userId, cancellationToken);

        if (freelancer == null)
        {
            return NotFound();
        }

        var skills = freelancer.FreelancerSkills
            .Select(fs => new SkillResponseDto
            {
                Id = fs.SkillId,
                Name = fs.Skill.Name
            })
            .OrderBy(s => s.Name)
            .ToList();

        return Ok(skills);
    }

    [HttpPost("{skillId:int}")]
    [Authorize(Roles = "Freelancer")]
    public async Task<IActionResult> AddSkill(int skillId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();

        var freelancer = await _context.FreelancerProfiles
            .FirstOrDefaultAsync(fp => fp.UserId == userId, cancellationToken);

        if (freelancer == null)
        {
            return NotFound();
        }

        var skill = await _context.Skills
            .FirstOrDefaultAsync(s => s.Id == skillId, cancellationToken);

        if (skill == null)
        {
            return BadRequest(new { message = "Skill does not exist." });
        }

        var existing = await _context.FreelancerSkills
            .FirstOrDefaultAsync(fs => fs.FreelancerId == freelancer.Id && fs.SkillId == skillId, cancellationToken);

        if (existing != null)
        {
            return BadRequest(new { message = "Skill already added to profile." });
        }

        var freelancerSkill = new FreelancerSkill
        {
            FreelancerId = freelancer.Id,
            SkillId = skillId
        };

        _context.FreelancerSkills.Add(freelancerSkill);
        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpDelete("{skillId:int}")]
    [Authorize(Roles = "Freelancer")]
    public async Task<IActionResult> RemoveSkill(int skillId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();

        var freelancer = await _context.FreelancerProfiles
            .FirstOrDefaultAsync(fp => fp.UserId == userId, cancellationToken);

        if (freelancer == null)
        {
            return NotFound();
        }

        var freelancerSkill = await _context.FreelancerSkills
            .FirstOrDefaultAsync(fs => fs.FreelancerId == freelancer.Id && fs.SkillId == skillId, cancellationToken);

        if (freelancerSkill == null)
        {
            return NotFound();
        }

        _context.FreelancerSkills.Remove(freelancerSkill);
        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static int GetUserIdFromClaims(ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? throw new UnauthorizedAccessException("User ID not found in claims.");
        return int.Parse(value);
    }

    private int GetUserId() => GetUserIdFromClaims(User);
}

