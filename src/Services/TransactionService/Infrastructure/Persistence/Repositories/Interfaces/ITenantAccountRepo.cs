using TransactionService.Domain.Entities;

namespace TransactionService.Infrastructure.Persistence.Repositories.Interfaces;

public interface ITenantAccountRepo
{
    Task<TenantAccount?> GetByTenantPublicIdAsync(
        Guid tenantPublicId,
        CancellationToken cancellationToken = default);

    Task AddAsync(TenantAccount account, CancellationToken cancellationToken = default);
}