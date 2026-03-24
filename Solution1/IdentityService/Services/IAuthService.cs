using IdentityService.DTOs;

namespace IdentityService.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> Register(RegisterRequestDto dto);
        Task<AuthResponseDto> Login(LoginRequestDto dto);
    }
}
