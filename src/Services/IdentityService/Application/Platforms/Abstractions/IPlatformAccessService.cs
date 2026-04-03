using IdentityService.Application.Platforms.Dtos;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Platforms.Abstractions;

public interface IPlatformAccessService
{
    Task<ServiceResult<PlatformTokenDto>> SelectPlatformAsync(
        string userPublicId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<bool>> HasPlatformAccessAsync(
        string userPublicId,
        CancellationToken cancellationToken = default);
}