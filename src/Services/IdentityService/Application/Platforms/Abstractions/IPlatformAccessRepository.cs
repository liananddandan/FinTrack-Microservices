using IdentityService.Domain.Entities;

namespace IdentityService.Application.Platforms.Abstractions;


public interface IPlatformAccessRepository
{
    Task<PlatformAccess?> GetByUserPublicIdAsync(
        string userPublicId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsEnabledAccessAsync(
        string userPublicId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        PlatformAccess platformAccess,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        PlatformAccess platformAccess,
        CancellationToken cancellationToken = default);
}