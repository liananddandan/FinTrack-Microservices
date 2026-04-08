using DotNetCore.CAP;
using IdentityService.Application.Tenants.Abstractions;
using SharedKernel.Contracts.Platform;
using SharedKernel.Topics;

namespace IdentityService.Infrastructure.Platform;

public class TenantDomainSubscriber(
    ITenantDomainProjectionSyncService syncService,
    ILogger<TenantDomainSubscriber> logger) : ICapSubscribe
{
    [CapSubscribe(PlatformTopics.TenantDomainUpserted)]
    public async Task HandleTenantDomainUpsertedAsync(
        TenantDomainUpsertedMessage message,
        CancellationToken cancellationToken)
    {
        var result = await syncService.UpsertAsync(message, cancellationToken);

        if (result.Success)
        {
            return;
        }

        logger.LogError(
            "Failed to upsert tenant domain projection. DomainPublicId: {DomainPublicId}, Host: {Host}, Code: {Code}, Message: {Message}",
            message.DomainPublicId,
            message.Host,
            result.Code,
            result.Message);

        // Business conflict: log and swallow, no retry
        if (result.Code == "Identity.TenantDomainProjection.HostConflict")
        {
            return;
        }

        // Transient/unknown failure: let CAP retry
        throw new InvalidOperationException(
            result.Message ?? "Failed to upsert tenant domain projection.");
    }

    [CapSubscribe(PlatformTopics.TenantDomainRemoved)]
    public async Task HandleTenantDomainRemovedAsync(
        TenantDomainRemovedMessage message,
        CancellationToken cancellationToken)
    {
        var result = await syncService.RemoveAsync(message, cancellationToken);

        if (!result.Success)
        {
            logger.LogError(
                "Failed to remove tenant domain projection. DomainPublicId: {DomainPublicId}, Code: {Code}, Message: {Message}",
                message.DomainPublicId,
                result.Code,
                result.Message);

            throw new InvalidOperationException(result.Message ?? "Failed to remove tenant domain projection.");
        }
    }
}