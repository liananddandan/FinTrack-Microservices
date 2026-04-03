using IdentityService.Application.Common.Abstractions;
using IdentityService.Application.Tenants.Abstractions;
using IdentityService.Domain.Entities;
using SharedKernel.Common.Results;
using SharedKernel.Contracts.Platform;

namespace IdentityService.Application.Tenants.Services;



public class TenantDomainProjectionSyncService(
    ITenantDomainProjectionRepository repository,
    IUnitOfWork unitOfWork)
    : ITenantDomainProjectionSyncService
{
    public async Task<ServiceResult<bool>> UpsertAsync(
        TenantDomainUpsertedMessage message,
        CancellationToken cancellationToken = default)
    {
        var normalizedHost = message.Host.Trim().ToLowerInvariant();

        var existing = await repository.GetByDomainPublicIdAsync(
            message.DomainPublicId,
            cancellationToken);

        if (existing is null)
        {
            var conflictByHost = await repository.GetByHostAsync(normalizedHost, cancellationToken);
            if (conflictByHost is not null && conflictByHost.DomainPublicId != message.DomainPublicId)
            {
                return ServiceResult<bool>.Fail(
                    "Identity.TenantDomainProjection.HostConflict",
                    $"Host '{normalizedHost}' is already bound to another domain projection.");
            }

            var projection = new TenantDomainProjection
            {
                DomainPublicId = message.DomainPublicId,
                TenantPublicId = message.TenantPublicId,
                Host = normalizedHost,
                DomainType = message.DomainType,
                IsPrimary = message.IsPrimary,
                IsActive = message.IsActive,
                LastSyncedAtUtc = DateTime.UtcNow
            };

            await repository.AddAsync(projection, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return ServiceResult<bool>.Ok(
                true,
                "Identity.TenantDomainProjection.UpsertSuccess",
                "Tenant domain projection created successfully.");
        }

        if (!string.Equals(existing.Host, normalizedHost, StringComparison.OrdinalIgnoreCase))
        {
            var conflictByHost = await repository.GetByHostAsync(normalizedHost, cancellationToken);
            if (conflictByHost is not null && conflictByHost.DomainPublicId != message.DomainPublicId)
            {
                return ServiceResult<bool>.Fail(
                    "Identity.TenantDomainProjection.HostConflict",
                    $"Host '{normalizedHost}' is already bound to another domain projection.");
            }
        }

        existing.TenantPublicId = message.TenantPublicId;
        existing.Host = normalizedHost;
        existing.DomainType = message.DomainType;
        existing.IsPrimary = message.IsPrimary;
        existing.IsActive = message.IsActive;
        existing.LastSyncedAtUtc = DateTime.UtcNow;

        repository.Update(existing);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<bool>.Ok(
            true,
            "Identity.TenantDomainProjection.UpsertSuccess",
            "Tenant domain projection updated successfully.");
    }

    public async Task<ServiceResult<bool>> RemoveAsync(
        TenantDomainRemovedMessage message,
        CancellationToken cancellationToken = default)
    {
        var existing = await repository.GetByDomainPublicIdAsync(
            message.DomainPublicId,
            cancellationToken);

        if (existing is null)
        {
            return ServiceResult<bool>.Ok(
                true,
                "Identity.TenantDomainProjection.RemoveIgnored",
                "Projection not found. Remove ignored.");
        }

        repository.Remove(existing);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<bool>.Ok(
            true,
            "Identity.TenantDomainProjection.RemoveSuccess",
            "Tenant domain projection removed successfully.");
    }
}