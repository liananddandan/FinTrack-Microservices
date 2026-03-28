using System.Security.Claims;
using SharedKernel.Common.DTOs.Auth;
using TransactionService.Application.Common.Abstractions;

namespace TransactionService.Infrastructure.Authentication;

public class CurrentTenantContext(IHttpContextAccessor httpContextAccessor) : ICurrentTenantContext
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Guid TenantPublicId
    {
        get
        {
            var value = User?.FindFirst(JwtClaimNames.Tenant)?.Value;

            return Guid.TryParse(value, out var tenantPublicId)
                ? tenantPublicId
                : Guid.Empty;
        }
    }

    public Guid UserPublicId
    {
        get
        {
            var value = User?.FindFirst(JwtClaimNames.UserId)?.Value;

            return Guid.TryParse(value, out var userPublicId)
                ? userPublicId
                : Guid.Empty;
        }
    }

    public string? Role =>
        User?.FindFirst(JwtClaimNames.Role)?.Value;

    public bool IsAuthenticated =>
        User?.Identity?.IsAuthenticated ?? false;
}