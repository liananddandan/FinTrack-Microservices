using Microsoft.EntityFrameworkCore;
using SharedKernel.Contracts.Payments;
using TransactionService.Application.Payments.Abstractions;
using TransactionService.Domain.Entities;

namespace TransactionService.Infrastructure.Persistence.Repositories;

public class PaymentRepository(TransactionDbContext dbContext) : IPaymentRepository
{
    public async Task AddAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        await dbContext.Payments.AddAsync(payment, cancellationToken);
    }

    public async Task<Payment?> GetByPublicIdAsync(Guid paymentPublicId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Payments
            .FirstOrDefaultAsync(x => x.PublicId == paymentPublicId, cancellationToken);
    }

    public async Task<List<Payment>> GetByOrderIdAsync(long orderId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Payments
            .Where(x => x.OrderId == orderId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsSucceededPaymentByOrderIdAsync(long orderId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Payments
            .AnyAsync(
                x => x.OrderId == orderId &&
                     !x.IsDeleted &&
                     x.Status == PaymentStatuses.Succeeded,
                cancellationToken);
    }

    public async Task<Payment?> GetByProviderPaymentIntentIdAsync(
        string providerPaymentIntentId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Payments
            .FirstOrDefaultAsync(
                x => x.ProviderPaymentIntentId == providerPaymentIntentId,
                cancellationToken);
    }
}