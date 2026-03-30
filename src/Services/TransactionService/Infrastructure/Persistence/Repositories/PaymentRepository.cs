using Microsoft.EntityFrameworkCore;
using TransactionService.Application.Payments.Abstractions;
using TransactionService.Domain.Entities;

namespace TransactionService.Infrastructure.Persistence.Repositories;

public class PaymentRepository(TransactionDbContext dbContext) : IPaymentRepository
{
    public async Task AddAsync(Payment payment, CancellationToken cancellationToken)
    {
        await dbContext.Payments.AddAsync(payment, cancellationToken);
    }

    public async Task<Payment?> GetByOrderIdAsync(
        long orderId,
        CancellationToken cancellationToken)
    {
        return await dbContext.Payments
            .FirstOrDefaultAsync(
                x => x.OrderId == orderId && !x.IsDeleted,
                cancellationToken);
    }

    public async Task<Payment?> GetByOrderPublicIdAsync(
        Guid tenantPublicId,
        Guid orderPublicId,
        CancellationToken cancellationToken)
    {
        return await dbContext.Payments
            .Include(x => x.Order)
            .FirstOrDefaultAsync(
                x => x.TenantPublicId == tenantPublicId &&
                     x.Order.PublicId == orderPublicId &&
                     !x.IsDeleted,
                cancellationToken);
    }

    public async Task<Payment?> GetByIdempotencyKeyAsync(
        string idempotencyKey,
        CancellationToken cancellationToken)
    {
        return await dbContext.Payments
            .FirstOrDefaultAsync(
                x => x.IdempotencyKey == idempotencyKey && !x.IsDeleted,
                cancellationToken);
    }
}