namespace TransactionService.Application.Payments.Abstractions;

public interface ITenantPaymentConfigService
{
    Task<string?> GetStripeConnectedAccountIdAsync(
        Guid tenantPublicId,
        CancellationToken cancellationToken = default);
}