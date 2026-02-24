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
public class ServicesController : ControllerBase
{
    private readonly AppDbContext _context;

    public ServicesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ServiceResponseDto>>> GetServices(
        [FromQuery] FreelancerCategory? category,
        [FromQuery] int? freelancerId,
        CancellationToken cancellationToken)
    {
        var query = _context.Services
            .Where(s => s.IsActive)
            .Include(s => s.Freelancer)
            .AsQueryable();

        if (category.HasValue)
            query = query.Where(s => s.Category == category.Value);

        if (freelancerId.HasValue)
            query = query.Where(s => s.FreelancerId == freelancerId.Value);

        var services = await query.ToListAsync(cancellationToken);
        return Ok(services.Select(MapToResponseDto));
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<ServiceResponseDto>> GetService(int id, CancellationToken cancellationToken)
    {
        var service = await _context.Services
            .Include(s => s.Freelancer)
            .FirstOrDefaultAsync(s => s.Id == id && s.IsActive, cancellationToken);

        if (service == null)
            return NotFound();

        return Ok(MapToResponseDto(service));
    }

    [HttpGet("me")]
    [Authorize(Roles = "Freelancer")]
    public async Task<ActionResult<IEnumerable<ServiceResponseDto>>> GetMyServices(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var profile = await _context.FreelancerProfiles
            .FirstOrDefaultAsync(fp => fp.UserId == userId, cancellationToken);

        if (profile == null)
            return NotFound();

        var services = await _context.Services
            .Include(s => s.Freelancer)
            .Where(s => s.FreelancerId == profile.Id)
            .ToListAsync(cancellationToken);

        return Ok(services.Select(MapToResponseDto));
    }

    [HttpPost]
    [Authorize(Roles = "Freelancer")]
    public async Task<ActionResult<ServiceResponseDto>> CreateService(
        CreateServiceDto dto,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var profile = await _context.FreelancerProfiles
            .FirstOrDefaultAsync(fp => fp.UserId == userId, cancellationToken);

        if (profile == null)
            return NotFound(new { message = "Freelancer profile not found." });

        if (profile.ApplicationStatus != FreelancerApplicationStatus.Approved)
            return StatusCode(403, new { message = "Your account must be approved before posting services." });

        var service = new Service
        {
            FreelancerId = profile.Id,
            Title = dto.Title,
            Description = dto.Description,
            Category = dto.Category,
            PricingType = dto.PricingType
        };

        _context.Services.Add(service);
        await _context.SaveChangesAsync(cancellationToken);

        await _context.Entry(service).Reference(s => s.Freelancer).LoadAsync(cancellationToken);

        return CreatedAtAction(nameof(GetService), new { id = service.Id }, MapToResponseDto(service));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Freelancer")]
    public async Task<ActionResult<ServiceResponseDto>> UpdateService(
        int id,
        UpdateServiceDto dto,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var profile = await _context.FreelancerProfiles
            .FirstOrDefaultAsync(fp => fp.UserId == userId, cancellationToken);

        if (profile == null)
            return NotFound();

        var service = await _context.Services
            .Include(s => s.Freelancer)
            .FirstOrDefaultAsync(s => s.Id == id && s.FreelancerId == profile.Id, cancellationToken);

        if (service == null)
            return NotFound();

        if (dto.Title != null) service.Title = dto.Title;
        if (dto.Description != null) service.Description = dto.Description;
        if (dto.Category.HasValue) service.Category = dto.Category.Value;
        if (dto.PricingType.HasValue) service.PricingType = dto.PricingType.Value;

        service.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(MapToResponseDto(service));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Freelancer")]
    public async Task<IActionResult> DeleteService(int id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var profile = await _context.FreelancerProfiles
            .FirstOrDefaultAsync(fp => fp.UserId == userId, cancellationToken);

        if (profile == null)
            return NotFound();

        var service = await _context.Services
            .FirstOrDefaultAsync(s => s.Id == id && s.FreelancerId == profile.Id, cancellationToken);

        if (service == null)
            return NotFound();

        _context.Services.Remove(service);
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

    private static ServiceResponseDto MapToResponseDto(Service s) => new()
    {
        Id = s.Id,
        FreelancerId = s.FreelancerId,
        FreelancerName = s.Freelancer.FullName,
        Title = s.Title,
        Description = s.Description,
        Category = s.Category.ToString(),
        PricingType = s.PricingType.ToString(),
        IsActive = s.IsActive,
        CreatedAt = s.CreatedAt
    };
}
