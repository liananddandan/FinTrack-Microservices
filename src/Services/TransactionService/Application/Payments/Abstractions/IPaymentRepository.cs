using TransactionService.Domain.Entities;

namespace TransactionService.Application.Payments.Abstractions;

public interface IPaymentRepository
{
    Task AddAsync(Payment payment, CancellationToken cancellationToken);

    Task<Payment?> GetByOrderIdAsync(
        long orderId,
        CancellationToken cancellationToken);

    Task<Payment?> GetByOrderPublicIdAsync(
        Guid tenantPublicId,
        Guid orderPublicId,
        CancellationToken cancellationToken);

    Task<Payment?> GetByIdempotencyKeyAsync(
        string idempotencyKey,
        CancellationToken cancellationToken);
    
    
    Task<Payment?> GetByProviderReferenceAsync(
        string providerPaymentReference,
        CancellationToken cancellationToken);
}