using System.ComponentModel.DataAnnotations;

namespace IdentityService.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public required string Name { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(150)]
    public required string Email { get; set; }

    [Required]
    [StringLength(500)]
    public required string PasswordHash { get; set; }

    [Required]
    [StringLength(50)]
    public required string Role { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? ResetOtp { get; set; }
    public DateTime? OtpExpiry { get; set; }
}