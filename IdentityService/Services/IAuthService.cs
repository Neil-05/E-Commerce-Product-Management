using IdentityService.DTOs;

namespace IdentityService.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> Register(RegisterRequestDto dto);
        Task<AuthResponseDto> Login(LoginRequestDto dto);
        Task ChangePassword(string email, ChangePasswordDto dto);
        Task ForgotPassword(string email);
        Task ResetPassword(ResetPasswordDto dto);

        void Logout(string token);
        bool IsTokenBlacklisted(string token);
    }
}
