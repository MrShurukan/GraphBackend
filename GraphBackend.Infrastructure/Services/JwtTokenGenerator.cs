using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GraphBackend.Application;
using GraphBackend.Domain.Models;
using GraphBackend.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GraphBackend.Infrastructure.Services;

public class JwtTokenGenerator(
    IOptions<JwtSettings> options) 
    : IJwtTokenGenerator
{
    private readonly JwtSettings _settings = options.Value;

    public string GenerateToken(int userId, string email, Roles role)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.Role, role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}