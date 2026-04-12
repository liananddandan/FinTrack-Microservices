using TransactionService.Domain.Entities;

namespace TransactionService.Application.Payments.Abstractions;

public interface IPaymentRepository
{
    Task AddAsync(Payment payment, CancellationToken cancellationToken = default);

    Task<Payment?> GetByPublicIdAsync(Guid paymentPublicId, CancellationToken cancellationToken = default);

    Task<List<Payment>> GetByOrderIdAsync(long orderId, CancellationToken cancellationToken = default);

    Task<bool> ExistsSucceededPaymentByOrderIdAsync(long orderId, CancellationToken cancellationToken = default);
    Task<Payment?> GetByProviderPaymentIntentIdAsync(
        string providerPaymentIntentId,
        CancellationToken cancellationToken = default);
}