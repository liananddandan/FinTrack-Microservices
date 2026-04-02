using System.Diagnostics;
using IdentityService.Application.Tenants.Abstractions;

namespace IdentityService.Application.Common.Middlewares;

public class TenantContextResolutionMiddleware
{
    public const string TenantRequestContextItemKey = "TenantRequestContext";

    private readonly RequestDelegate _next;

    public TenantContextResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ITenantContextResolver tenantContextResolver,
        ILogger<TenantContextResolutionMiddleware> logger)
    {
        var host = context.Request.Host.Host;

        if (!string.IsNullOrWhiteSpace(host))
        {
            var tenantContext = await tenantContextResolver.ResolveAsync(
                host,
                context.RequestAborted);

            if (tenantContext is not null)
            {
                context.Items[TenantRequestContextItemKey] = tenantContext;

                Activity.Current?.SetTag("tenant.id", tenantContext.TenantPublicId.ToString());
                Activity.Current?.SetTag("tenant.host", tenantContext.Host);
                Activity.Current?.SetTag("tenant.domain_type", tenantContext.DomainType);

                using (logger.BeginScope(new Dictionary<string, object>
                       {
                           ["TenantPublicId"] = tenantContext.TenantPublicId,
                           ["TenantHost"] = tenantContext.Host,
                           ["TenantDomainType"] = tenantContext.DomainType
                       }))
                {
                    await _next(context);
                    return;
                }
            }
        }

        await _next(context);
    }
}