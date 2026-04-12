namespace IdentityService.Application.Tenants.Dtos;

public sealed record TenantStripeConnectStatusDto(
    string? ConnectedAccountId,
    bool ChargesEnabled,
    bool PayoutsEnabled,
    bool IsConnected,
    bool OnboardingRequired);
    
public sealed record CreateTenantStripeOnboardingLinkDto(
    string Url);