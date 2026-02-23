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
public class ClientsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ClientsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<ClientProfileResponseDto>> GetClient(int id, CancellationToken cancellationToken)
    {
        var client = await _context.ClientProfiles
            .FirstOrDefaultAsync(cp => cp.Id == id, cancellationToken);

        if (client == null)
        {
            return NotFound();
        }

        return Ok(MapToResponseDto(client));
    }

    [HttpGet("me")]
    [Authorize(Roles = "Client")]
    public async Task<ActionResult<ClientProfileResponseDto>> GetMe(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var client = await _context.ClientProfiles
            .FirstOrDefaultAsync(cp => cp.UserId == userId, cancellationToken);

        if (client == null)
        {
            return NotFound();
        }

        return Ok(MapToResponseDto(client));
    }

    [HttpPost("profile")]
    [Authorize(Roles = "Client")]
    public async Task<ActionResult<ClientProfileResponseDto>> CreateProfile(
        CreateClientProfileDto dto,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var existingProfile = await _context.ClientProfiles
            .FirstOrDefaultAsync(cp => cp.UserId == userId, cancellationToken);

        if (existingProfile != null)
        {
            return BadRequest(new { message = "Profile already exists for this user." });
        }

        var profile = new ClientProfile
        {
            UserId = userId,
            DisplayName = dto.DisplayName,
            About = dto.About,
            Website = dto.Website,
            AvatarUrl = dto.AvatarUrl
        };

        _context.ClientProfiles.Add(profile);
        await _context.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetClient), new { id = profile.Id }, MapToResponseDto(profile));
    }

    [HttpPut("me")]
    [Authorize(Roles = "Client")]
    public async Task<ActionResult<ClientProfileResponseDto>> UpdateMe(
        UpdateClientProfileDto dto,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var profile = await _context.ClientProfiles
            .FirstOrDefaultAsync(cp => cp.UserId == userId, cancellationToken);

        if (profile == null)
        {
            return NotFound();
        }

        if (dto.DisplayName != null) profile.DisplayName = dto.DisplayName;
        if (dto.About != null) profile.About = dto.About;
        if (dto.Website != null) profile.Website = dto.Website;
        if (dto.AvatarUrl != null) profile.AvatarUrl = dto.AvatarUrl;

        profile.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(MapToResponseDto(profile));
    }

    private static int GetUserIdFromClaims(ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? throw new UnauthorizedAccessException("User ID not found in claims.");
        return int.Parse(value);
    }

    private int GetUserId() => GetUserIdFromClaims(User);

    private static ClientProfileResponseDto MapToResponseDto(ClientProfile cp)
    {
        return new ClientProfileResponseDto
        {
            Id = cp.Id,
            UserId = cp.UserId,
            DisplayName = cp.DisplayName,
            About = cp.About,
            Website = cp.Website,
            AvatarUrl = cp.AvatarUrl,
            CreatedAt = cp.CreatedAt
        };
    }
}
