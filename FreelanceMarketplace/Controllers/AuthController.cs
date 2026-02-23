using Microsoft.EntityFrameworkCore;
using FreelanceMarketplace.Data;
using FreelanceMarketplace.DTOs;
using FreelanceMarketplace.Models;
using FreelanceMarketplace.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceMarketplace.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly TokenService _tokenService;

    public AuthController(AppDbContext context, TokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto, CancellationToken cancellationToken)
    {
        var emailExists = await _context.Users.AnyAsync(u => u.Email == dto.Email, cancellationToken);
        if (emailExists)
        {
            return BadRequest(new { message = "An account with this email already exists." });
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        var isActive = dto.Role == UserRole.Client;

        var user = new User
        {
            Email = dto.Email,
            PasswordHash = passwordHash,
            Role = dto.Role,
            IsActive = isActive
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        if (dto.Role == UserRole.Freelancer)
        {
            return StatusCode(201, new { message = "Registration successful. Your account is pending admin approval." });
        }

        var token = _tokenService.GenerateToken(user.Id, user.Email, user.Role.ToString());

        return StatusCode(201, new AuthResponseDto
        {
            Token = token,
            Role = user.Role.ToString(),
            UserId = user.Id,
            Email = user.Email
        });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email, cancellationToken);
        if (user == null)
        {
            return Unauthorized();
        }

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            return Unauthorized();
        }

        if (!user.IsActive)
        {
            return StatusCode(403, "Account pending approval");
        }

        var token = _tokenService.GenerateToken(user.Id, user.Email, user.Role.ToString());

        return Ok(new AuthResponseDto
        {
            Token = token,
            Role = user.Role.ToString(),
            UserId = user.Id,
            Email = user.Email
        });
    }
}
