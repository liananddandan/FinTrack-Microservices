namespace IdentityService.Common.Status;

public static class JwtTokenCustomKeys
{
    public const string UserPublicIdKey = "UserPublicId";
    public const string JwtVersionKey = "JwtVersion";
    public const string TenantPublicIdKey = "TenantPublicId";
    public const string UserRoleInTenant = "UserRoleInTenant";
    public const string AccessTokenTypeKey = "AccessTokenType";
    public const string AccessTokenTypeValue = "AccessToken";
    public const string RefreshTokenTypeKey = "RefreshTokenType";
    public const string RefreshTokenTypeValue = "RefreshToken";
    public const string FirstLoginTokenKey = "FirstLoginTokenType";
    public const string FirstLoginTokenValue = "FirstLoginToken";
}