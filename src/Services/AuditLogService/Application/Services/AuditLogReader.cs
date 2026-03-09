using AuditLogService.Application.DTOs;
using AuditLogService.Application.Interfaces;
using AuditLogService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AuditLogService.Application.Services;

public class AuditLogReader(AuditLogDbContext dbContext) : IAuditLogReader
{
    public async Task<PagedResult<AuditLogDto>> QueryAsync(
        AuditLogQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.AuditLogs.AsNoTracking()
            .Where(x => x.TenantPublicId == request.TenantPublicId);

        if (!string.IsNullOrWhiteSpace(request.ActionType))
            query = query.Where(x => x.ActionType == request.ActionType);

        if (!string.IsNullOrWhiteSpace(request.ActorUserPublicId))
            query = query.Where(x => x.ActorUserPublicId == request.ActorUserPublicId);

        if (!string.IsNullOrWhiteSpace(request.TargetPublicId))
            query = query.Where(x => x.TargetPublicId == request.TargetPublicId);

        if (request.FromUtc.HasValue)
            query = query.Where(x => x.OccurredAtUtc >= request.FromUtc.Value);

        if (request.ToUtc.HasValue)
            query = query.Where(x => x.OccurredAtUtc <= request.ToUtc.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.OccurredAtUtc)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new AuditLogDto
            {
                PublicId = x.PublicId.ToString(),
                TenantPublicId = x.TenantPublicId,
                ActorUserPublicId = x.ActorUserPublicId,
                ActorDisplayName = x.ActorDisplayName,
                ActionType = x.ActionType,
                Category = x.Category,
                TargetType = x.TargetType,
                TargetPublicId = x.TargetPublicId,
                TargetDisplay = x.TargetDisplay,
                Source = x.Source,
                CorrelationId = x.CorrelationId,
                OccurredAtUtc = x.OccurredAtUtc,
                MetadataJson = x.MetadataJson,
                Summary = BuildSummary(x.ActionType, x.ActorDisplayName, x.TargetDisplay)
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<AuditLogDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    private static string BuildSummary(string actionType, string? actorDisplayName, string? targetDisplay)
    {
        var actor = string.IsNullOrWhiteSpace(actorDisplayName) ? "System" : actorDisplayName;
        var target = string.IsNullOrWhiteSpace(targetDisplay) ? "target" : targetDisplay;

        return actionType switch
        {
            "Membership.Invited" => $"{actor} invited {target}.",
            "Membership.Removed" => $"{actor} removed {target}.",
            "Membership.RoleChanged" => $"{actor} changed the role of {target}.",
            _ => $"{actor} performed {actionType} on {target}."
        };
    }
}