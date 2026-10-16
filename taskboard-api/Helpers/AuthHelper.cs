using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TaskboardApi.Models;

namespace TaskboardApi.Helpers;

public static class AuthHelper
{
    public static string GenerateSalt(int size = 16)
    {
        var bytes = RandomNumberGenerator.GetBytes(size);
        return Convert.ToHexString(bytes);
    }

    public static string HashPassword(string password, string salt)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password + salt));
        return Convert.ToHexString(bytes);
    }

    public static bool Verify(string password, string salt, string hash)
        => HashPassword(password, salt) == hash;

    public static string NormalizeRole(string? role)
        => string.Equals(role, "Manager", StringComparison.OrdinalIgnoreCase) ? "Manager" : "Member";

    public static string GenerateJwt(User user, IConfiguration config)
    {
        var key = config["Jwt:Key"] ?? "DevOnly-Change-Me-TaskBoard-0123456789ABCDEF";
        var issuer = config["Jwt:Issuer"] ?? "TaskBoard";
        var audience = config["Jwt:Audience"] ?? "TaskBoard";
        var minutes = int.TryParse(config["Jwt:ExpiresMinutes"], out var m) ? m : 720;
        var creds = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };
        var token = new JwtSecurityToken(issuer, audience, claims,
            expires: DateTime.UtcNow.AddMinutes(minutes), signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
