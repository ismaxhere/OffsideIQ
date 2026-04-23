using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OffsideIQ.Core.DTOs;
using OffsideIQ.Core.Entities;
using OffsideIQ.Core.Interfaces;

namespace OffsideIQ.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IConfiguration _config;

    public AuthService(IUserRepository users, IConfiguration config)
    {
        _users = users;
        _config = config;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (await _users.EmailExistsAsync(request.Email))
            throw new InvalidOperationException("Email is already registered.");

        var user = new User
        {
            Email = request.Email.ToLower().Trim(),
            DisplayName = request.DisplayName.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
        };

        await _users.AddAsync(user);

        var dto = ToDto(user);
        return new AuthResponse(GenerateJwtToken(dto), GenerateRefreshToken(), dto);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _users.GetByEmailAsync(request.Email)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        user.LastLoginAt = DateTime.UtcNow;
        await _users.UpdateAsync(user);

        var dto = ToDto(user);
        return new AuthResponse(GenerateJwtToken(dto), GenerateRefreshToken(), dto);
    }

    public string GenerateJwtToken(UserDto user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _config["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured")));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.DisplayName),
            new Claim(ClaimTypes.Role, user.Role),
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken() =>
        Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));

    private static UserDto ToDto(User user) => new(user.Id, user.Email, user.DisplayName, user.Role);
}
