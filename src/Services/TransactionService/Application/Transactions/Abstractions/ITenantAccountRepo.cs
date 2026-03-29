using TransactionService.Domain.Entities;

namespace TransactionService.Application.Transactions.Abstractions;

public interface ITenantAccountRepo
{
    Task<TenantAccount?> GetByTenantPublicIdAsync(
        Guid tenantPublicId,
        CancellationToken cancellationToken = default);

    Task AddAsync(TenantAccount account, CancellationToken cancellationToken = default);
}