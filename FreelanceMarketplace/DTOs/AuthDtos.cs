using System.ComponentModel.DataAnnotations;
using FreelanceMarketplace.Models;

namespace FreelanceMarketplace.DTOs;

public class RegisterDto
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [MinLength(6)]
    public required string Password { get; set; }

    [Required]
    public UserRole Role { get; set; }
}

public class LoginDto
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    public required string Password { get; set; }
}

public class AuthResponseDto
{
    public required string Token { get; set; }
    public required string Role { get; set; }
    public int UserId { get; set; }
    public required string Email { get; set; }
}
