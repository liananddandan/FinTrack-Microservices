using AuditLogService.Application.DTOs;

namespace AuditLogService.Application.Interfaces;

public interface IAuditLogReader
{
    Task<PagedResult<AuditLogDto>> QueryAsync(
        AuditLogQueryRequest request,
        CancellationToken cancellationToken = default);
}