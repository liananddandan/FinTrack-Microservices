using SharedKernel.Contracts.AuditLogs;

namespace IdentityService.Application.Abstractions;

public interface IAuditLogPublisher
{
    Task PublishAsync(
        string topic,
        AuditLogMessage message,
        CancellationToken cancellationToken = default);
}