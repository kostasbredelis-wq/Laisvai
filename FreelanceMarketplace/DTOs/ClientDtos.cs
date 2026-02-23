using System.ComponentModel.DataAnnotations;

namespace FreelanceMarketplace.DTOs;

public class CreateClientProfileDto
{
    [Required]
    [MaxLength(200)]
    public required string DisplayName { get; set; }

    [MaxLength(2000)]
    public string? About { get; set; }

    [MaxLength(500)]
    public string? Website { get; set; }

    [MaxLength(500)]
    public string? AvatarUrl { get; set; }
}

public class UpdateClientProfileDto
{
    [MaxLength(200)]
    public string? DisplayName { get; set; }

    [MaxLength(2000)]
    public string? About { get; set; }

    [MaxLength(500)]
    public string? Website { get; set; }

    [MaxLength(500)]
    public string? AvatarUrl { get; set; }
}

public class ClientProfileResponseDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public required string DisplayName { get; set; }
    public string? About { get; set; }
    public string? Website { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}
