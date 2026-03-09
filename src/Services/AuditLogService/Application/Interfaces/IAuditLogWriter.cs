using SharedKernel.Contracts.AuditLogs;

namespace AuditLogService.Application.Interfaces;

public interface IAuditLogWriter
{
    Task WriteAsync(AuditLogMessage message, CancellationToken cancellationToken = default);
}