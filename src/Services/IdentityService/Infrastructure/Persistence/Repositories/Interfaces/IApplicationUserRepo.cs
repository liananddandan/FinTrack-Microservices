using IdentityService.Domain.Entities;

namespace IdentityService.Infrastructure.Persistence.Repositories.Interfaces;

public interface IApplicationUserRepo
{
    Task IncreaseJwtVersion(ApplicationUser user, CancellationToken cancellationToken = default);
    Task<ApplicationUser?> GetUserByEmailWithMembershipsAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> IsEmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<ApplicationUser?> GetUserByPublicIdWithMembershipsAsync(
        string userPublicId,
        CancellationToken cancellationToken = default);
}