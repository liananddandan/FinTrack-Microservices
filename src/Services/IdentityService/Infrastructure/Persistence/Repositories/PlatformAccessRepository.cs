using IdentityService.Application.Platforms.Abstractions;
using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Persistence.Repositories;

public class PlatformAccessRepository(
    ApplicationIdentityDbContext dbContext) : IPlatformAccessRepository
{
    public async Task<PlatformAccess?> GetByUserPublicIdAsync(
        string userPublicId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.PlatformAccesses
            .FirstOrDefaultAsync(
                x => x.UserPublicId == userPublicId,
                cancellationToken);
    }

    public async Task<bool> ExistsEnabledAccessAsync(
        string userPublicId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.PlatformAccesses
            .AnyAsync(
                x => x.UserPublicId == userPublicId && x.IsEnabled,
                cancellationToken);
    }

    public async Task AddAsync(
        PlatformAccess platformAccess,
        CancellationToken cancellationToken = default)
    {
        await dbContext.PlatformAccesses.AddAsync(platformAccess, cancellationToken);
    }

    public Task UpdateAsync(
        PlatformAccess platformAccess,
        CancellationToken cancellationToken = default)
    {
        dbContext.PlatformAccesses.Update(platformAccess);
        return Task.CompletedTask;
    }
}