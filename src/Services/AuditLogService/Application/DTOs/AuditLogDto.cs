namespace AuditLogService.Application.DTOs;

public class AuditLogDto
{
    public string PublicId { get; set; } = string.Empty;
    public string TenantPublicId { get; set; } = string.Empty;

    public string? ActorUserPublicId { get; set; }
    public string? ActorDisplayName { get; set; }

    public string ActionType { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;

    public string? TargetType { get; set; }
    public string? TargetPublicId { get; set; }
    public string? TargetDisplay { get; set; }

    public string? Source { get; set; }
    public string? CorrelationId { get; set; }

    public DateTime OccurredAtUtc { get; set; }
    public string MetadataJson { get; set; } = "{}";

    public string Summary { get; set; } = string.Empty;
}