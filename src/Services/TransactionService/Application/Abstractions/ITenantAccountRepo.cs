using TransactionService.Domain.Entities;

namespace TransactionService.Application.Abstractions;

public interface ITenantAccountRepo
{
    Task<TenantAccount?> GetByTenantPublicIdAsync(
        Guid tenantPublicId,
        CancellationToken cancellationToken = default);

    Task AddAsync(TenantAccount account, CancellationToken cancellationToken = default);
}