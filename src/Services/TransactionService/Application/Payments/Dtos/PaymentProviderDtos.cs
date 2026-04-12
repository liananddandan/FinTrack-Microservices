namespace TransactionService.Application.Payments.Dtos;

public sealed record CreateProviderPaymentRequest(
    string PaymentPublicId,
    string OrderPublicId,
    Guid TenantPublicId,
    decimal Amount,
    string Currency,
    string? ConnectedAccountId);

public sealed record CreateProviderPaymentResult(
    string? ExternalPaymentId,
    string? ExternalChargeId,
    string? ClientSecret,
    string InitialStatus,
    string? FailureReason);