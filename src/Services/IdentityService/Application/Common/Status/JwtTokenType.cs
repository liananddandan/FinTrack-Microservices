namespace IdentityService.Application.Common.Status;

public enum JwtTokenType
{
    AccountAccessToken = 1,
    TenantAccessToken = 2,
    RefreshToken,
    InvitationToken
}