using SharedKernel.Common.Results;
using SharedKernel.Contracts.Platform;

namespace IdentityService.Application.Tenants.Abstractions;

public interface ITenantDomainProjectionSyncService
{
    Task<ServiceResult<bool>> UpsertAsync(
        TenantDomainUpsertedMessage message,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<bool>> RemoveAsync(
        TenantDomainRemovedMessage message,
        CancellationToken cancellationToken = default);
}