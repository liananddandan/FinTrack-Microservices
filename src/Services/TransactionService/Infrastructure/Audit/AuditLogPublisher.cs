using DotNetCore.CAP;
using SharedKernel.Contracts.AuditLogs;
using TransactionService.Application.Common.Abstractions;

namespace TransactionService.Infrastructure.Audit;

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