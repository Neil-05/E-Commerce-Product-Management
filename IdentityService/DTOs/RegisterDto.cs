using System.ComponentModel.DataAnnotations;
namespace IdentityService.DTOs
{
    public class RegisterRequestDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }

        [Required]
        [RegularExpression("Admin|User", ErrorMessage = "Role must be Admin or User")]
        public string Role { get; set; }
    }
}

