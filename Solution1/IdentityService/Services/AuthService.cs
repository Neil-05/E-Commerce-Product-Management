using IdentityService.DTOs;
using IdentityService.Entities;
using IdentityService.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IdentityService.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _repo;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUserRepository repo, IConfiguration config, ILogger<AuthService> logger)
    {
        _repo = repo;
        _config = config;
        _logger = logger;
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

        _logger.LogInformation("User registered: {email}", dto.Email);

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
}