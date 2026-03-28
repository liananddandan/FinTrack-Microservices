using IdentityService.Application.Abstractions;
using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Persistence.Repositories;

public class ApplicationUserRepo(ApplicationIdentityDbContext dbContext) : IApplicationUserRepo
{
    public Task IncreaseJwtVersion(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        user.JwtVersion += 1;
        return Task.CompletedTask;
    }

    public async Task<ApplicationUser?> GetUserByEmailWithMembershipsAsync(string email,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .Include(u => u.Memberships)
            .ThenInclude(m => m.Tenant)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<bool> IsEmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        email = email.Trim().ToLowerInvariant();

        return await dbContext.Users
            .AnyAsync(u => u.Email == email, cancellationToken);
    }

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

    public async Task<ApplicationUser?> GetUserByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        email = email.Trim().ToLowerInvariant();

        return await dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<ApplicationUser?> GetUserByPublicIdAsync(
        string publicId,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(publicId, out var parsedPublicId))
        {
            return null;
        }

        return await dbContext.Users
            .FirstOrDefaultAsync(
                u => u.PublicId == parsedPublicId,
                cancellationToken);
    }
}