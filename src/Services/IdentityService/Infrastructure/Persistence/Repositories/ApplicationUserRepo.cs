using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Persistence.Repositories;

public class ApplicationUserRepo(ApplicationIdentityDbContext dbContext) : IApplicationUserRepo
{
    public Task IncreaseJwtVersion(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        user.JwtVersion += 1;
        return Task.CompletedTask;
    }

    public async Task<ApplicationUser?> GetUserByEmailWithMembershipsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .Include(u => u.Memberships)
            .ThenInclude(m => m.Tenant)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<bool> IsEmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .AnyAsync(u => u.Email == email, cancellationToken);    }

    public async Task<ApplicationUser?> GetUserByPublicIdWithMembershipsAsync(
        string userPublicId,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(userPublicId, out var parsedUserPublicId))
        {
            return null;
        }

        return await dbContext.Users
            .Include(u => u.Memberships)
            .ThenInclude(m => m.Tenant)
            .FirstOrDefaultAsync(u => u.PublicId == parsedUserPublicId, cancellationToken);
    }
}