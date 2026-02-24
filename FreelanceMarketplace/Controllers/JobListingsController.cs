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
public class JobListingsController : ControllerBase
{
    private readonly AppDbContext _context;

    public JobListingsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<JobListingResponseDto>>> GetJobListings(
        [FromQuery] FreelancerCategory? category,
        [FromQuery] int? skillId,
        [FromQuery] bool? isOpen,
        CancellationToken cancellationToken)
    {
        var query = _context.JobListings
            .Include(j => j.Client)
            .Include(j => j.JobListingSkills)
                .ThenInclude(js => js.Skill)
            .AsQueryable();

        if (isOpen.HasValue)
            query = query.Where(j => j.IsOpen == isOpen.Value);
        else
            query = query.Where(j => j.IsOpen);

        if (category.HasValue)
            query = query.Where(j => j.Category == category.Value);

        if (skillId.HasValue)
            query = query.Where(j => j.JobListingSkills.Any(js => js.SkillId == skillId.Value));

        var listings = await query.ToListAsync(cancellationToken);
        return Ok(listings.Select(MapToResponseDto));
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<JobListingResponseDto>> GetJobListing(int id, CancellationToken cancellationToken)
    {
        var listing = await _context.JobListings
            .Include(j => j.Client)
            .Include(j => j.JobListingSkills)
                .ThenInclude(js => js.Skill)
            .FirstOrDefaultAsync(j => j.Id == id, cancellationToken);

        if (listing == null)
            return NotFound();

        return Ok(MapToResponseDto(listing));
    }

    [HttpGet("me")]
    [Authorize(Roles = "Client")]
    public async Task<ActionResult<IEnumerable<JobListingResponseDto>>> GetMyJobListings(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var client = await _context.ClientProfiles
            .FirstOrDefaultAsync(cp => cp.UserId == userId, cancellationToken);

        if (client == null)
            return NotFound();

        var listings = await _context.JobListings
            .Include(j => j.Client)
            .Include(j => j.JobListingSkills)
                .ThenInclude(js => js.Skill)
            .Where(j => j.ClientId == client.Id)
            .ToListAsync(cancellationToken);

        return Ok(listings.Select(MapToResponseDto));
    }

    [HttpPost]
    [Authorize(Roles = "Client")]
    public async Task<ActionResult<JobListingResponseDto>> CreateJobListing(
        CreateJobListingDto dto,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var client = await _context.ClientProfiles
            .FirstOrDefaultAsync(cp => cp.UserId == userId, cancellationToken);

        if (client == null)
            return NotFound(new { message = "Client profile not found." });

        if (dto.SkillIds != null && dto.SkillIds.Count > 0)
        {
            var existingSkillCount = await _context.Skills
                .CountAsync(s => dto.SkillIds.Contains(s.Id), cancellationToken);

            if (existingSkillCount != dto.SkillIds.Count)
                return BadRequest(new { message = "One or more skill IDs are invalid." });
        }

        var listing = new JobListing
        {
            ClientId = client.Id,
            Title = dto.Title,
            Description = dto.Description,
            Category = dto.Category,
            BudgetType = dto.BudgetType,
            Deadline = dto.Deadline
        };

        _context.JobListings.Add(listing);
        await _context.SaveChangesAsync(cancellationToken);

        if (dto.SkillIds != null && dto.SkillIds.Count > 0)
        {
            var skills = dto.SkillIds.Select(skillId => new JobListingSkill
            {
                JobListingId = listing.Id,
                SkillId = skillId
            });
            _context.JobListingSkills.AddRange(skills);
            await _context.SaveChangesAsync(cancellationToken);
        }

        await _context.Entry(listing).Reference(j => j.Client).LoadAsync(cancellationToken);
        await _context.Entry(listing)
            .Collection(j => j.JobListingSkills)
            .Query()
            .Include(js => js.Skill)
            .LoadAsync(cancellationToken);

        return CreatedAtAction(nameof(GetJobListing), new { id = listing.Id }, MapToResponseDto(listing));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Client")]
    public async Task<ActionResult<JobListingResponseDto>> UpdateJobListing(
        int id,
        UpdateJobListingDto dto,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var client = await _context.ClientProfiles
            .FirstOrDefaultAsync(cp => cp.UserId == userId, cancellationToken);

        if (client == null)
            return NotFound();

        var listing = await _context.JobListings
            .Include(j => j.Client)
            .Include(j => j.JobListingSkills)
                .ThenInclude(js => js.Skill)
            .FirstOrDefaultAsync(j => j.Id == id && j.ClientId == client.Id, cancellationToken);

        if (listing == null)
            return NotFound();

        if (dto.Title != null) listing.Title = dto.Title;
        if (dto.Description != null) listing.Description = dto.Description;
        if (dto.Category.HasValue) listing.Category = dto.Category.Value;
        if (dto.BudgetType.HasValue) listing.BudgetType = dto.BudgetType.Value;
        if (dto.Deadline.HasValue) listing.Deadline = dto.Deadline;

        if (dto.SkillIds != null)
        {
            var existingSkillCount = await _context.Skills
                .CountAsync(s => dto.SkillIds.Contains(s.Id), cancellationToken);

            if (existingSkillCount != dto.SkillIds.Count)
                return BadRequest(new { message = "One or more skill IDs are invalid." });

            _context.JobListingSkills.RemoveRange(listing.JobListingSkills);
            var newSkills = dto.SkillIds.Select(skillId => new JobListingSkill
            {
                JobListingId = listing.Id,
                SkillId = skillId
            });
            _context.JobListingSkills.AddRange(newSkills);
        }

        listing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        await _context.Entry(listing)
            .Collection(j => j.JobListingSkills)
            .Query()
            .Include(js => js.Skill)
            .LoadAsync(cancellationToken);

        return Ok(MapToResponseDto(listing));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> DeleteJobListing(int id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var client = await _context.ClientProfiles
            .FirstOrDefaultAsync(cp => cp.UserId == userId, cancellationToken);

        if (client == null)
            return NotFound();

        var listing = await _context.JobListings
            .FirstOrDefaultAsync(j => j.Id == id && j.ClientId == client.Id, cancellationToken);

        if (listing == null)
            return NotFound();

        var skills = await _context.JobListingSkills
            .Where(js => js.JobListingId == id)
            .ToListAsync(cancellationToken);
        _context.JobListingSkills.RemoveRange(skills);

        var applications = await _context.Applications
            .Where(a => a.JobListingId == id)
            .ToListAsync(cancellationToken);
        _context.Applications.RemoveRange(applications);

        _context.JobListings.Remove(listing);
        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpPatch("{id:int}/close")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> CloseJobListing(int id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var client = await _context.ClientProfiles
            .FirstOrDefaultAsync(cp => cp.UserId == userId, cancellationToken);

        if (client == null)
            return NotFound();

        var listing = await _context.JobListings
            .FirstOrDefaultAsync(j => j.Id == id && j.ClientId == client.Id, cancellationToken);

        if (listing == null)
            return NotFound();

        listing.IsOpen = false;
        listing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private int GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? throw new UnauthorizedAccessException("User ID not found in claims.");
        return int.Parse(value);
    }

    private static JobListingResponseDto MapToResponseDto(JobListing j) => new()
    {
        Id = j.Id,
        ClientId = j.ClientId,
        ClientName = j.Client.DisplayName,
        Title = j.Title,
        Description = j.Description,
        Category = j.Category.ToString(),
        BudgetType = j.BudgetType.ToString(),
        Deadline = j.Deadline,
        IsOpen = j.IsOpen,
        CreatedAt = j.CreatedAt,
        Skills = j.JobListingSkills.Select(js => js.Skill.Name).ToList()
    };
}
