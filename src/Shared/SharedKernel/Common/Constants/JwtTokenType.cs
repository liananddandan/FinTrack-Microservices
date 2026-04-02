namespace SharedKernel.Common.Constants;

public enum JwtTokenType
{
    AccountAccessToken = 1,
    TenantAccessToken = 2,
    PlatformAccessToken = 3,
    RefreshToken = 4,
    InvitationToken = 5
}