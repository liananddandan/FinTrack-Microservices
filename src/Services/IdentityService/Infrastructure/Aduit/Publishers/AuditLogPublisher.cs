using DotNetCore.CAP;
using IdentityService.Application.Common.Abstractions;
using SharedKernel.Contracts.AuditLogs;

namespace IdentityService.Infrastructure.Aduit.Publishers;

public class AuditLogPublisher(ICapPublisher capPublisher,
    ILogger<AuditLogPublisher> logger) : IAuditLogPublisher
{
    public async Task PublishAsync(
        string topic,
        AuditLogMessage message,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Publishing audit event. Topic={Topic}, Action={Action}, Tenant={Tenant}, Target={Target}",
            topic,
            message.ActionType,
            message.TenantPublicId,
            message.TargetPublicId
        );
        await capPublisher.PublishAsync(topic, message, cancellationToken: cancellationToken);
    }
}