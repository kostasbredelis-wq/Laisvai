using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FreelanceMarketplace.Data;
using FreelanceMarketplace.DTOs;
using FreelanceMarketplace.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FreelanceMarketplace.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FreelancersController : ControllerBase
{
    private readonly AppDbContext _context;

    public FreelancersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<FreelancerProfileResponseDto>>> GetFreelancers(
        [FromQuery] FreelancerCategory? category,
        [FromQuery] int? skillId,
        [FromQuery] double? minRating,
        [FromQuery] string? sort,
        CancellationToken cancellationToken)
    {
        var query = _context.FreelancerProfiles
            .Where(fp => fp.ApplicationStatus == FreelancerApplicationStatus.Approved)
            .Include(fp => fp.FreelancerSkills)
                .ThenInclude(fs => fs.Skill)
            .Include(fp => fp.ReviewsReceived)
            .AsQueryable();

        if (category.HasValue)
        {
            query = query.Where(fp => fp.Category == category.Value);
        }

        if (skillId.HasValue)
        {
            query = query.Where(fp => fp.FreelancerSkills.Any(fs => fs.SkillId == skillId.Value));
        }

        if (minRating.HasValue)
        {
            query = query.Where(fp => !fp.ReviewsReceived.Any() ||
                fp.ReviewsReceived.Average(r => r.Rating) >= minRating.Value);
        }

        var freelancers = await query.ToListAsync(cancellationToken);

        freelancers = sort?.ToLowerInvariant() switch
        {
            "highest_rated" => freelancers
                .OrderByDescending(fp => fp.ReviewsReceived.Count > 0
                    ? fp.ReviewsReceived.Average(r => r.Rating)
                    : 0)
                .ThenByDescending(fp => fp.CreatedAt)
                .ToList(),
            "newest" or _ => freelancers.OrderByDescending(fp => fp.CreatedAt).ToList()
        };

        return Ok(freelancers.Select(MapToResponseDto));
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<FreelancerProfileResponseDto>> GetFreelancer(int id, CancellationToken cancellationToken)
    {
        var userId = User.Identity?.IsAuthenticated == true ? GetUserId() : (int?)null;

        var freelancer = await _context.FreelancerProfiles
            .Include(fp => fp.FreelancerSkills)
                .ThenInclude(fs => fs.Skill)
            .Include(fp => fp.ReviewsReceived)
            .FirstOrDefaultAsync(fp => fp.Id == id, cancellationToken);

        if (freelancer == null)
        {
            return NotFound();
        }

        var isOwner = userId.HasValue && freelancer.UserId == userId.Value;
        if (!isOwner && freelancer.ApplicationStatus != FreelancerApplicationStatus.Approved)
        {
            return NotFound();
        }

        return Ok(MapToResponseDto(freelancer));
    }

    [HttpGet("me")]
    [Authorize(Roles = "Freelancer")]
    public async Task<ActionResult<FreelancerProfileResponseDto>> GetMe(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var freelancer = await _context.FreelancerProfiles
            .Include(fp => fp.FreelancerSkills)
                .ThenInclude(fs => fs.Skill)
            .Include(fp => fp.ReviewsReceived)
            .FirstOrDefaultAsync(fp => fp.UserId == userId, cancellationToken);

        if (freelancer == null)
        {
            return NotFound();
        }

        return Ok(MapToResponseDto(freelancer));
    }

    [HttpPost("profile")]
    [Authorize(Roles = "Freelancer")]
    public async Task<ActionResult<FreelancerProfileResponseDto>> CreateProfile(
        CreateFreelancerProfileDto dto,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var existingProfile = await _context.FreelancerProfiles
            .FirstOrDefaultAsync(fp => fp.UserId == userId, cancellationToken);

        if (existingProfile != null)
        {
            return BadRequest(new { message = "Profile already exists for this user." });
        }

        var profile = new FreelancerProfile
        {
            UserId = userId,
            FullName = dto.FullName,
            Bio = dto.Bio,
            Category = dto.Category,
            PortfolioUrl = dto.PortfolioUrl,
            AvatarUrl = dto.AvatarUrl
        };

        _context.FreelancerProfiles.Add(profile);
        await _context.SaveChangesAsync(cancellationToken);

        await _context.Entry(profile)
            .Collection(fp => fp.FreelancerSkills)
            .Query()
            .Include(fs => fs.Skill)
            .LoadAsync(cancellationToken);
        await _context.Entry(profile)
            .Collection(fp => fp.ReviewsReceived)
            .LoadAsync(cancellationToken);

        return CreatedAtAction(nameof(GetFreelancer), new { id = profile.Id }, MapToResponseDto(profile));
    }

    [HttpPut("me")]
    [Authorize(Roles = "Freelancer")]
    public async Task<ActionResult<FreelancerProfileResponseDto>> UpdateMe(
        UpdateFreelancerProfileDto dto,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var profile = await _context.FreelancerProfiles
            .Include(fp => fp.FreelancerSkills)
                .ThenInclude(fs => fs.Skill)
            .Include(fp => fp.ReviewsReceived)
            .FirstOrDefaultAsync(fp => fp.UserId == userId, cancellationToken);

        if (profile == null)
        {
            return NotFound();
        }

        if (dto.FullName != null) profile.FullName = dto.FullName;
        if (dto.Bio != null) profile.Bio = dto.Bio;
        if (dto.Category.HasValue) profile.Category = dto.Category.Value;
        if (dto.PortfolioUrl != null) profile.PortfolioUrl = dto.PortfolioUrl;
        if (dto.AvatarUrl != null) profile.AvatarUrl = dto.AvatarUrl;

        profile.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(MapToResponseDto(profile));
    }

    private static int GetUserId(ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? throw new UnauthorizedAccessException("User ID not found in claims.");
        return int.Parse(value);
    }

    private int GetUserId() => GetUserId(User);

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
