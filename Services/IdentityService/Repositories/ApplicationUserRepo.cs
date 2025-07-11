using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Repositories.Interfaces;

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
}