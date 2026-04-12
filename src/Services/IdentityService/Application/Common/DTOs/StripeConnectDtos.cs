namespace IdentityService.Application.Common.DTOs;

public sealed record CreateConnectedAccountResult(
    string ConnectedAccountId,
    bool ChargesEnabled,
    bool PayoutsEnabled);

public sealed record CreateAccountOnboardingLinkResult(
    string Url);

public sealed record StripeConnectedAccountStatusResult(
    bool ChargesEnabled,
    bool PayoutsEnabled);