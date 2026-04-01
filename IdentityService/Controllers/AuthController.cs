using IdentityService.DTOs;
using IdentityService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _service;

    public AuthController(IAuthService service)
    {
        _service = service;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequestDto dto)
    {
        var result = await _service.Register(dto);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto dto)
    {
        var result = await _service.Login(dto);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin-only")]
    public IActionResult AdminOnly()
    {
        return Ok("Only Admin can access");
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;

        await _service.ChangePassword(email, dto);
        return Ok("Password changed");
    }
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
    {
        await _service.ForgotPassword(dto.Email);
        return Ok("OTP sent to email");
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
    {
        await _service.ResetPassword(dto);
        return Ok("Password reset successful");
    }

    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        var token = Request.Headers["Authorization"]
            .ToString()
            .Replace("Bearer ", "");

        _service.Logout(token);

        return Ok("Logged out");
    }
}