using SharedKernel.Contracts.AuditLogs;

namespace IdentityService.Application.Services.Interfaces;

public interface IAuditLogPublisher
{
    Task PublishAsync(
        string topic,
        AuditLogMessage message,
        CancellationToken cancellationToken = default);
}