using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TransactionService.Filters;

public class RequireClaimsFilter : IAsyncAuthorizationFilter
{
    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any())
        {
            return Task.CompletedTask;
        }
        
        var tenantPublicId = context.HttpContext.User.FindFirst("TenantPublicId")?.Value;
        var userPublicId = context.HttpContext.User.FindFirst("UserPublicId")?.Value;
        var jwtVersion = context.HttpContext.User.FindFirst("JwtVersion")?.Value;
        var userRoleInTenant = context.HttpContext.User.FindFirst("UserRoleInTenant")?.Value;
        if (string.IsNullOrEmpty(tenantPublicId) 
            || string.IsNullOrEmpty(userPublicId)
            || string.IsNullOrEmpty(userRoleInTenant)
            || string.IsNullOrEmpty(jwtVersion))
        {
            context.Result = new UnauthorizedResult();
        }
        return Task.CompletedTask;
    }
}