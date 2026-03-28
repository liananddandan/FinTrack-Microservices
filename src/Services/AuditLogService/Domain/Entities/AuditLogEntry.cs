namespace AuditLogService.Domain.Entities;

public class AuditLogEntry
{
    public long Id { get; set; }
    public Guid PublicId { get; set; } = Guid.NewGuid();

    public required string TenantPublicId { get; set; }

    public string? ActorUserPublicId { get; set; }
    public string? ActorDisplayName { get; set; }

    public required string ActionType { get; set; }
    public required string Category { get; set; }

    public string? TargetType { get; set; }
    public string? TargetPublicId { get; set; }
    public string? TargetDisplay { get; set; }

    public string? Source { get; set; }
    public string? CorrelationId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    public DateTime OccurredAtUtc { get; set; }
    public required string MetadataJson { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}