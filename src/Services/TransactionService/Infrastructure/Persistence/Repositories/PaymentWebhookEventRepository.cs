using Microsoft.EntityFrameworkCore;
using TransactionService.Application.Payments.Abstractions;
using TransactionService.Domain.Entities;

namespace TransactionService.Infrastructure.Persistence.Repositories;

public class PaymentWebhookEventRepository(TransactionDbContext dbContext)
    : IPaymentWebhookEventRepository
{
    public async Task<bool> ExistsAsync(
        string provider,
        string eventId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.PaymentWebhookEvents.AnyAsync(
            x => x.Provider == provider && x.EventId == eventId,
            cancellationToken);
    }

    public async Task AddAsync(
        PaymentWebhookEvent paymentWebhookEvent,
        CancellationToken cancellationToken = default)
    {
        await dbContext.PaymentWebhookEvents.AddAsync(paymentWebhookEvent, cancellationToken);
    }
}