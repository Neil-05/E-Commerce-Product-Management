namespace IdentityService.DTOs
{
    public class RefreshTokenRequestDto
    {
        public string Email { get; set; }
        public string RefreshToken { get; set; }
    }
}
