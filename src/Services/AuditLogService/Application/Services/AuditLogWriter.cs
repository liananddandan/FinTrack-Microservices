using System.Text.Json;
using AuditLogService.Application.Interfaces;
using AuditLogService.Domain.Entities;
using AuditLogService.Infrastructure.Persistence;
using SharedKernel.Contracts.AuditLogs;

namespace AuditLogService.Application.Services;

public class AuditLogWriter(AuditLogDbContext dbContext) : IAuditLogWriter
{
    public async Task WriteAsync(AuditLogMessage message, CancellationToken cancellationToken = default)
    {
        var entity = new AuditLogEntry
        {
            TenantPublicId = message.TenantPublicId,
            ActorUserPublicId = message.ActorUserPublicId,
            ActorDisplayName = message.ActorDisplayName,
            ActionType = message.ActionType,
            Category = message.Category,
            TargetType = message.TargetType,
            TargetPublicId = message.TargetPublicId,
            TargetDisplay = message.TargetDisplay,
            Source = message.Source,
            CorrelationId = message.CorrelationId,
            IpAddress = message.IpAddress,
            UserAgent = message.UserAgent,
            OccurredAtUtc = message.OccurredAtUtc,
            MetadataJson = JsonSerializer.Serialize(message.Metadata)
        };

        dbContext.AuditLogs.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}