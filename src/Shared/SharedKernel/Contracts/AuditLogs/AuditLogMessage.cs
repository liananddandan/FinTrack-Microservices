namespace SharedKernel.Contracts.AuditLogs;

public record AuditLogMessage
{
    public required string TenantPublicId { get; init; }
    public string? ActorUserPublicId { get; init; }
    public string? ActorDisplayName { get; init; }

    public required string ActionType { get; init; }
    public required string Category { get; init; }

    public string? TargetType { get; init; }
    public string? TargetPublicId { get; init; }
    public string? TargetDisplay { get; init; }

    public string? Source { get; init; }
    public string? CorrelationId { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }

    public DateTime OccurredAtUtc { get; init; } = DateTime.UtcNow;

    public IReadOnlyList<AuditMetadataItem> Metadata { get; init; } = [];
}