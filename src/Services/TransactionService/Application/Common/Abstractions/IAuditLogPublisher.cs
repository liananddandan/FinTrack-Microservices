using SharedKernel.Contracts.AuditLogs;

namespace TransactionService.Application.Common.Abstractions;

public interface IAuditLogPublisher
{
    Task PublishAsync(
        string topic,
        AuditLogMessage message,
        CancellationToken cancellationToken = default);
}