using Microsoft.EntityFrameworkCore;
using TransactionService.Application.Transactions.Abstractions;
using TransactionService.Domain.Entities;

namespace TransactionService.Infrastructure.Persistence.Repositories;

public class TenantAccountRepo(TransactionDbContext dbContext) : ITenantAccountRepo
{
    public async Task<TenantAccount?> GetByTenantPublicIdAsync(
        Guid tenantPublicId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.TenantAccounts
            .FirstOrDefaultAsync(x => x.TenantPublicId == tenantPublicId, cancellationToken);
    }

    public async Task AddAsync(TenantAccount account, CancellationToken cancellationToken = default)
    {
        await dbContext.TenantAccounts.AddAsync(account, cancellationToken);
    }
}