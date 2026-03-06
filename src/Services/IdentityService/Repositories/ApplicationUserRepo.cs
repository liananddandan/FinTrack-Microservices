using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Repositories;

public class ApplicationUserRepo(ApplicationIdentityDbContext dbContext) : IApplicationUserRepo
{
    public Task ChangeFirstLoginStatus(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        user.IsFirstLogin = false;
        return Task.CompletedTask;
    }

    public Task IncreaseJwtVersion(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        user.JwtVersion += 1;
        return Task.CompletedTask;
    }

    public async Task<ApplicationUser?> GetUserByEmailWithTenant(string email, CancellationToken cancellationToken = default)
    {
        return await dbContext.Users.Include(u => u.Tenant)
            .Where(u => email.Equals(u.Email))
            .FirstOrDefaultAsync(cancellationToken);
    }
}