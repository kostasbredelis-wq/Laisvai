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
public class ApplicationsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ApplicationsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("~/api/jobs/{jobId:int}/apply")]
    [Authorize(Roles = "Freelancer")]
    public async Task<ActionResult<ApplicationResponseDto>> Apply(
        int jobId,
        CreateApplicationDto dto,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var profile = await _context.FreelancerProfiles
            .FirstOrDefaultAsync(fp => fp.UserId == userId, cancellationToken);

        if (profile == null)
            return NotFound();

        if (profile.ApplicationStatus != FreelancerApplicationStatus.Approved)
            return StatusCode(403, new { message = "Your profile must be approved before applying to jobs." });

        var listing = await _context.JobListings
            .FirstOrDefaultAsync(j => j.Id == jobId && j.IsOpen, cancellationToken);

        if (listing == null)
            return NotFound();

        var alreadyApplied = await _context.Applications
            .AnyAsync(a => a.JobListingId == jobId && a.FreelancerId == profile.Id, cancellationToken);

        if (alreadyApplied)
            return Conflict(new { message = "You have already applied to this job listing." });

        var application = new Application
        {
            JobListingId = jobId,
            FreelancerId = profile.Id,
            CoverMessage = dto.CoverMessage,
            Status = ApplicationStatus.Pending
        };

        _context.Applications.Add(application);
        await _context.SaveChangesAsync(cancellationToken);

        await _context.Entry(application).Reference(a => a.JobListing).LoadAsync(cancellationToken);
        await _context.Entry(application).Reference(a => a.Freelancer).LoadAsync(cancellationToken);

        return CreatedAtAction(nameof(GetMyApplications), null, MapToResponseDto(application));
    }

    [HttpGet("~/api/jobs/{jobId:int}/applications")]
    [Authorize(Roles = "Client")]
    public async Task<ActionResult<IEnumerable<ApplicationResponseDto>>> GetJobApplications(
        int jobId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var client = await _context.ClientProfiles
            .FirstOrDefaultAsync(cp => cp.UserId == userId, cancellationToken);

        if (client == null)
            return NotFound();

        var listing = await _context.JobListings
            .FirstOrDefaultAsync(j => j.Id == jobId, cancellationToken);

        if (listing == null)
            return NotFound();

        if (listing.ClientId != client.Id)
            return StatusCode(403, new { message = "You do not own this job listing." });

        var applications = await _context.Applications
            .Include(a => a.JobListing)
            .Include(a => a.Freelancer)
            .Where(a => a.JobListingId == jobId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        return Ok(applications.Select(MapToResponseDto));
    }

    [HttpGet("me")]
    [Authorize(Roles = "Freelancer")]
    public async Task<ActionResult<IEnumerable<ApplicationResponseDto>>> GetMyApplications(
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var profile = await _context.FreelancerProfiles
            .FirstOrDefaultAsync(fp => fp.UserId == userId, cancellationToken);

        if (profile == null)
            return NotFound();

        var applications = await _context.Applications
            .Include(a => a.JobListing)
            .Include(a => a.Freelancer)
            .Where(a => a.FreelancerId == profile.Id)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        return Ok(applications.Select(MapToResponseDto));
    }

    [HttpPatch("{id:int}/accept")]
    [Authorize(Roles = "Client")]
    public async Task<ActionResult<ApplicationResponseDto>> AcceptApplication(
        int id,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var client = await _context.ClientProfiles
            .FirstOrDefaultAsync(cp => cp.UserId == userId, cancellationToken);

        if (client == null)
            return NotFound();

        var application = await _context.Applications
            .Include(a => a.JobListing)
            .Include(a => a.Freelancer)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (application == null)
            return NotFound();

        if (application.JobListing.ClientId != client.Id)
            return StatusCode(403, new { message = "You do not own this job listing." });

        if (application.Status != ApplicationStatus.Pending)
            return BadRequest(new { message = "Only pending applications can be accepted." });

        application.Status = ApplicationStatus.Accepted;
        application.UpdatedAt = DateTime.UtcNow;

        _context.Conversations.Add(new Conversation
        {
            FreelancerId = application.FreelancerId,
            ClientId = client.Id,
            JobListingId = application.JobListingId,
            ApplicationId = application.Id
        });

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(MapToResponseDto(application));
    }

    [HttpPatch("{id:int}/reject")]
    [Authorize(Roles = "Client")]
    public async Task<ActionResult<ApplicationResponseDto>> RejectApplication(
        int id,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var client = await _context.ClientProfiles
            .FirstOrDefaultAsync(cp => cp.UserId == userId, cancellationToken);

        if (client == null)
            return NotFound();

        var application = await _context.Applications
            .Include(a => a.JobListing)
            .Include(a => a.Freelancer)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (application == null)
            return NotFound();

        if (application.JobListing.ClientId != client.Id)
            return StatusCode(403, new { message = "You do not own this job listing." });

        if (application.Status != ApplicationStatus.Pending)
            return BadRequest(new { message = "Only pending applications can be rejected." });

        application.Status = ApplicationStatus.Rejected;
        application.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(MapToResponseDto(application));
    }

    private int GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? throw new UnauthorizedAccessException("User ID not found in claims.");
        return int.Parse(value);
    }

    private static ApplicationResponseDto MapToResponseDto(Application a) => new()
    {
        Id = a.Id,
        JobListingId = a.JobListingId,
        JobTitle = a.JobListing.Title,
        FreelancerId = a.FreelancerId,
        FreelancerName = a.Freelancer.FullName,
        CoverMessage = a.CoverMessage,
        Status = a.Status.ToString(),
        CreatedAt = a.CreatedAt,
        UpdatedAt = a.UpdatedAt
    };
}
