using IdentityService.Domain.Entities;

namespace IdentityService.Repositories.Interfaces;

public interface IApplicationUserRepo
{
    Task ChangeFirstLoginStatus(ApplicationUser user, CancellationToken cancellationToken = default);
    Task IncreaseJwtVersion(ApplicationUser user, CancellationToken cancellationToken = default);
    Task<ApplicationUser?> GetUserByEmailWithTenant(string email, CancellationToken cancellationToken = default);
}