namespace SharedKernel.Common.Constants;

public enum JwtTokenType
{
    AccountAccessToken = 1,
    TenantAccessToken = 2,
    PlatformAccessToken = 3,
    RefreshToken = 4,
    // TODO: Refactor invitation token flow
    // - Remove custom "Authorization: Invite xxx" header handling
    // - Keep Authorization header only for Bearer JWT authentication
    // - Pass invitation token via query string or request body instead
    // - Remove InviteParseResult from HttpContext.Items
    // - Move invitation token parsing into application/service layer
    // - Validate invitation token for existence, expiration, version, and used status
    // - Keep invitation token as a one-time business token, not an authentication token
    // - Simplify related filters/middlewares by removing Invite token branch
    // - Replace InvitationParseDto usage with direct service result where possible
    // - Consider using AcceptInvitationRequest { Token } for accept endpoint
    InvitationToken = 5
}