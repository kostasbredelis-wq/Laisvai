using FreelanceMarketplace.Data;
using FreelanceMarketplace.DTOs;
using FreelanceMarketplace.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FreelanceMarketplace.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("freelancers/pending")]
    public async Task<ActionResult<IEnumerable<FreelancerProfileResponseDto>>> GetPendingFreelancers(
        CancellationToken cancellationToken)
    {
        var freelancers = await _context.FreelancerProfiles
            .Where(fp => fp.ApplicationStatus == FreelancerApplicationStatus.Pending)
            .Include(fp => fp.FreelancerSkills)
                .ThenInclude(fs => fs.Skill)
            .Include(fp => fp.ReviewsReceived)
            .OrderBy(fp => fp.CreatedAt)
            .ToListAsync(cancellationToken);

        return Ok(freelancers.Select(MapToResponseDto));
    }

    [HttpPatch("freelancers/{id:int}/approve")]
    public async Task<ActionResult<FreelancerProfileResponseDto>> ApproveFreelancer(
        int id,
        CancellationToken cancellationToken)
    {
        var freelancer = await _context.FreelancerProfiles
            .Include(fp => fp.FreelancerSkills)
                .ThenInclude(fs => fs.Skill)
            .Include(fp => fp.ReviewsReceived)
            .Include(fp => fp.User)
            .FirstOrDefaultAsync(fp => fp.Id == id, cancellationToken);

        if (freelancer == null)
            return NotFound();

        if (freelancer.ApplicationStatus != FreelancerApplicationStatus.Pending)
            return BadRequest(new { message = "Freelancer is not in a pending state." });

        freelancer.ApplicationStatus = FreelancerApplicationStatus.Approved;
        freelancer.User.IsActive = true;
        freelancer.User.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(MapToResponseDto(freelancer));
    }

    [HttpPatch("freelancers/{id:int}/reject")]
    public async Task<ActionResult<FreelancerProfileResponseDto>> RejectFreelancer(
        int id,
        CancellationToken cancellationToken)
    {
        var freelancer = await _context.FreelancerProfiles
            .Include(fp => fp.FreelancerSkills)
                .ThenInclude(fs => fs.Skill)
            .Include(fp => fp.ReviewsReceived)
            .FirstOrDefaultAsync(fp => fp.Id == id, cancellationToken);

        if (freelancer == null)
            return NotFound();

        if (freelancer.ApplicationStatus != FreelancerApplicationStatus.Pending)
            return BadRequest(new { message = "Freelancer is not in a pending state." });

        freelancer.ApplicationStatus = FreelancerApplicationStatus.Rejected;
        freelancer.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(MapToResponseDto(freelancer));
    }

    [HttpPatch("users/{id:int}/deactivate")]
    public async Task<IActionResult> DeactivateUser(int id, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user == null)
            return NotFound();

        if (!user.IsActive)
            return BadRequest(new { message = "User is already deactivated." });

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static FreelancerProfileResponseDto MapToResponseDto(FreelancerProfile fp)
    {
        var averageRating = fp.ReviewsReceived.Count > 0
            ? fp.ReviewsReceived.Average(r => r.Rating)
            : 0;

        return new FreelancerProfileResponseDto
        {
            Id = fp.Id,
            UserId = fp.UserId,
            FullName = fp.FullName,
            Bio = fp.Bio,
            Category = fp.Category.ToString(),
            PortfolioUrl = fp.PortfolioUrl,
            AvatarUrl = fp.AvatarUrl,
            ApplicationStatus = fp.ApplicationStatus.ToString(),
            AverageRating = Math.Round(averageRating, 2),
            ReviewCount = fp.ReviewsReceived.Count,
            CreatedAt = fp.CreatedAt,
            Skills = fp.FreelancerSkills.Select(fs => fs.Skill.Name).ToList()
        };
    }
}
