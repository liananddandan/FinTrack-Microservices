using TransactionService.Domain.Entities;

namespace TransactionService.Application.Payments.Abstractions;

public interface IPaymentWebhookEventRepository
{
    Task<bool> ExistsAsync(
        string provider,
        string eventId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        PaymentWebhookEvent paymentWebhookEvent,
        CancellationToken cancellationToken = default);
}