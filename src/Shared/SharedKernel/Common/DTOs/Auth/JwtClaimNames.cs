using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SharedKernel.Common.DTOs.Auth;

public static class JwtClaimNames
{
    public const string UserId = JwtRegisteredClaimNames.Sub;
    public const string JwtVersion = "jwtVersion";
    public const string TokenType = "type";

    public const string Tenant = "tenant";
    public const string TenantName = "tenantName";

    public const string Role = ClaimTypes.Role;
    public const string UserName = ClaimTypes.Name;
    public const string UserEmail = ClaimTypes.Email;
    public const string InvitationPublicId = "invitationPublicId";
    public const string InvitationVersion = "invitationVersion";

}