using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SharedKernel.Common.DTOs.Auth;

namespace TransactionService.Tests;

public static class JwtTestTokenFactory
{
    private const string Secret = "very-strong-secret-key-for-hmac256";
    private const string Issuer = "FinTrack";
    private const string Audience = "FinTrackAudience";

    public static string CreateTenantAccessToken(
        string userPublicId,
        string tenantPublicId,
        string role = "Member",
        string jwtVersion = "1")
    {
        var claims = new List<Claim>
        {
            new(JwtClaimNames.UserId, userPublicId),
            new(JwtClaimNames.Tenant, tenantPublicId),
            new(ClaimTypes.Role, role),
            new(JwtClaimNames.TokenType, "TenantAccessToken"),
            new(JwtClaimNames.JwtVersion, jwtVersion)
        };

        return CreateToken(claims);
    }

    public static string CreateAccountAccessToken(
        string userPublicId,
        string jwtVersion = "1")
    {
        var claims = new List<Claim>
        {
            new(JwtClaimNames.UserId, userPublicId),
            new(JwtClaimNames.TokenType, "AccountAccessToken"),
            new(JwtClaimNames.JwtVersion, jwtVersion)
        };

        return CreateToken(claims);
    }

    private static string CreateToken(IEnumerable<Claim> claims)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}