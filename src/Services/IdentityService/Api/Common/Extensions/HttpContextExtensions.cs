using IdentityService.Api.Common.Middlewares;
using IdentityService.Application.Common.DTOs;
using IdentityService.Application.Tenants.Dtos;

namespace IdentityService.Api.Common.Extensions;

public static class HttpContextExtensions
{
    public static JwtParseDto? GetHttpHeaderJwtParseResult(this HttpContext httpContext)
    {
        return httpContext.Items["JwtParseResult"] as JwtParseDto;
    }
    
    public static InvitationParseDto? GetHttpHeaderInviteParseResult(this HttpContext httpContext)
    {
        return httpContext.Items["InviteParseResult"] as InvitationParseDto;
    }
    
    public static TenantRequestContext? GetTenantRequestContext(this HttpContext httpContext)
    {
        if (httpContext.Items.TryGetValue(
                TenantContextResolutionMiddleware.TenantRequestContextItemKey,
                out var value) &&
            value is TenantRequestContext tenantContext)
        {
            return tenantContext;
        }

        return null;
    }
}