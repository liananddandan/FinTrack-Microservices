namespace SharedKernel.Common.Constants;

public enum JwtTokenType
{
    AccountAccessToken = 1,
    TenantAccessToken = 2,
    RefreshToken,
    InvitationToken
}