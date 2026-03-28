namespace AuditLogService.Application.DTOs;

public class AuditLogQueryRequest
{
    public required string TenantPublicId { get; set; }

    public string? ActionType { get; set; }
    public string? ActorUserPublicId { get; set; }
    public string? TargetPublicId { get; set; }

    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}