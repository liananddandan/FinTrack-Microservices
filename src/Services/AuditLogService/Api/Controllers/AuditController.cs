using System.Security.Claims;
using AuditLogService.Application.DTOs;
using AuditLogService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Common.DTOs;
using SharedKernel.Common.DTOs.Auth;

namespace AuditLogService.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/audit-logs")]
public class AuditLogsController(IAuditLogReader reader) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> QueryAsync(
        [FromQuery] string? actionType,
        [FromQuery] string? actorUserPublicId,
        [FromQuery] string? targetPublicId,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var tenantPublicId = User.FindFirst(JwtClaimNames.Tenant)?.Value;
        var role = User.FindFirst(JwtClaimNames.Role)?.Value;

        if (string.IsNullOrWhiteSpace(tenantPublicId))
        {
            return Unauthorized();
        }

        if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return Forbid();
        }

        var result = await reader.QueryAsync(
            new AuditLogQueryRequest
            {
                TenantPublicId = tenantPublicId,
                ActionType = actionType,
                ActorUserPublicId = actorUserPublicId,
                TargetPublicId = targetPublicId,
                FromUtc = fromUtc,
                ToUtc = toUtc,
                PageNumber = pageNumber,
                PageSize = pageSize
            },
            cancellationToken);

        return Ok(new ApiResponse<PagedResult<AuditLogDto>>(
            "AuditLogs.Query.Success",
            "Audit logs fetched successfully.",
            result));
    }
    
    [AllowAnonymous]
    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok("AuditLogService is running.");
    }
}