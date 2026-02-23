using System.ComponentModel.DataAnnotations;

namespace FreelanceMarketplace.Models;

/// <summary>User account for authentication. Auth is separated from profile data.</summary>
public class User
{
    public int Id { get; set; }

    [Required]
    [MaxLength(256)]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [MaxLength(256)]
    public required string PasswordHash { get; set; }

    [Required]
    public UserRole Role { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public FreelancerProfile? FreelancerProfile { get; set; }

    public ClientProfile? ClientProfile { get; set; }

    public ICollection<Message> SentMessages { get; set; } = new List<Message>();
}
