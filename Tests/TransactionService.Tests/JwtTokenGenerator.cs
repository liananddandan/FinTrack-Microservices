using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace TransactionService.Tests;

public class JwtTokenGenerator
{
    public static string GenerateFakeAccessToken(string userPublicId,
        string jwtVersion,
        string tenantPublicId,
        string userRoleInTenant)
    {
        var claims = new List<Claim>()
        {
            new("UserPublicId", userPublicId),
            new("JwtVersion", jwtVersion),
            new("TenantPublicId", tenantPublicId),
            new("UserRoleInTenant", userRoleInTenant),
            new("TokenType", "AccessToken")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("very-strong-secret-key-for-hmac256"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expire = DateTime.UtcNow.AddMinutes(60);
        var token = new JwtSecurityToken(
            "FinTrack",
            "FinTrackAudience",
            claims,
            DateTime.UtcNow,
            expire,
            credentials
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}