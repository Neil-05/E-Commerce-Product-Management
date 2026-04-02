using IdentityService.DTOs;
using IdentityService.Entities;
using IdentityService.Repositories;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IdentityService.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _repo;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthService> _logger;
    private readonly IHttpContextAccessor _httpContext;

    public AuthService(IUserRepository repo, IConfiguration config, ILogger<AuthService> logger, IHttpContextAccessor httpContext)
    {
        _repo = repo;
        _config = config;
        _logger = logger;
        _httpContext = httpContext;
    }


    public async Task<AuthResponseDto> Register(RegisterRequestDto dto)
    {
        // ✅ Single DB call
        if (await _repo.UserExistsAsync(dto.Email))
        {
            _logger.LogWarning("Duplicate registration attempt: {email}", dto.Email);
            throw new Exception("User already exists");
        }

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = dto.Role
        };

        await _repo.AddUserAsync(user);

        _logger.LogInformation("User registered: {email}",  dto.Email);

        return new AuthResponseDto
        {
            Token = GenerateJwt(user),
            Email = user.Email,
            Role = user.Role
        };
    }

    public async Task<AuthResponseDto> Login(LoginRequestDto dto)
    {
        var user = await _repo.GetByEmailAsync(dto.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            _logger.LogWarning("Invalid login attempt: {email}", dto.Email);
            throw new Exception("Invalid credentials");
        }

        _logger.LogInformation("User logged in: {email}", dto.Email);

        return new AuthResponseDto
        {
            Token = GenerateJwt(user),
            Email = user.Email,
            Role = user.Role
        };
    }

    private string GenerateJwt(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(ClaimTypes.Name, user.Email)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)
        );

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "auth",
            audience: "auth",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    public async Task ChangePassword(string email, ChangePasswordDto dto)
    {
        var user = await _repo.GetByEmailAsync(email);

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            throw new Exception("Wrong current password");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

        await _repo.UpdateUserAsync(user);
    }
    public async Task ForgotPassword(string email)
    {
        var user = await _repo.GetByEmailAsync(email);
        if (user == null)
            throw new Exception("User not found");

        var otp = new Random().Next(100000, 999999).ToString();

        user.ResetOtp = otp;
        user.OtpExpiry = DateTime.UtcNow.AddMinutes(10);

        await _repo.UpdateUserAsync(user);

        // Send email here
        await SendEmail(email, otp);
    }
    public async Task ResetPassword(ResetPasswordDto dto)
    {
        var user = await _repo.GetByEmailAsync(dto.Email);

        if (user.ResetOtp != dto.Otp || user.OtpExpiry < DateTime.UtcNow)
            throw new Exception("Invalid or expired OTP");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

        user.ResetOtp = null;
        user.OtpExpiry = null;

        await _repo.UpdateUserAsync(user);
    }
    private async Task SendEmail(string to, string otp)
    {
        var smtp = new System.Net.Mail.SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            Credentials = new System.Net.NetworkCredential("zorogoat05@gmail.com", "ubco msai cvde ayco"),
            EnableSsl = true,
        };

        var mail = new System.Net.Mail.MailMessage
        {
            From = new System.Net.Mail.MailAddress("your_email@gmail.com"),
            Subject = "Password Reset OTP",
            Body = $"Your OTP is: {otp}",
        };

        mail.To.Add(to);

        await smtp.SendMailAsync(mail);
    }

    private static HashSet<string> _blacklist = new();

    public void Logout(string token)
    {
        _blacklist.Add(token);
    }

    public bool IsTokenBlacklisted(string token)
    {
        return _blacklist.Contains(token);
    }
}