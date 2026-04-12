namespace TransactionService.Application.Payments.Dtos;

public sealed record CreateStripePaymentIntentResult(
    string PaymentIntentId,
    string? ChargeId,
    string ClientSecret,
    string Status);
    
public sealed record TenantStripeConnectStatusDto(
    string? ConnectedAccountId,
    bool ChargesEnabled,
    bool PayoutsEnabled,
    bool IsConnected,
    bool OnboardingRequired);