using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GameClubManager.Shared.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace GameClubManager.Server.Services;

public interface IJwtService
{
    string GenerateToken(User user);
    string GenerateToken(User user, int expirationHours);
}

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(User user)
    {
        // Получаем время жизни токена из конфигурации
        var expirationMinutes = int.Parse(_configuration["JwtSettings:ExpirationInMinutes"] ?? "60");
        return GenerateTokenWithExpiration(user, DateTime.Now.AddMinutes(expirationMinutes));
    }

    public string GenerateToken(User user, int expirationHours)
    {
        return GenerateTokenWithExpiration(user, DateTime.Now.AddHours(expirationHours));
    }

    private string GenerateTokenWithExpiration(User user, DateTime expiration)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: expiration,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
} 