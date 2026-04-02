using System.Diagnostics;
using IdentityService.Application.Tenants.Abstractions;

namespace IdentityService.Api.Common.Middlewares;

public class TenantContextResolutionMiddleware(RequestDelegate next)
{
    public const string TenantRequestContextItemKey = "TenantRequestContext";

    public async Task InvokeAsync(
        HttpContext context,
        ITenantContextResolver tenantContextResolver,
        ILogger<TenantContextResolutionMiddleware> logger)
    {
        var host = context.Request.Host.Host;

        logger.LogInformation(
            "TenantContextResolutionMiddleware started. Host: {Host}, Path: {Path}",
            host,
            context.Request.Path);

        if (!string.IsNullOrWhiteSpace(host))
        {
            try
            {
                logger.LogInformation(
                    "Resolving tenant context for host: {Host}",
                    host);

                var tenantContext = await tenantContextResolver.ResolveAsync(
                    host,
                    context.RequestAborted);

                if (tenantContext is not null)
                {
                    logger.LogInformation(
                        "Tenant context resolved successfully. TenantPublicId: {TenantPublicId}, Host: {TenantHost}, DomainType: {DomainType}",
                        tenantContext.TenantPublicId,
                        tenantContext.Host,
                        tenantContext.DomainType);

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
                        await next(context);

                        logger.LogInformation(
                            "Request completed with tenant context. StatusCode: {StatusCode}",
                            context.Response.StatusCode);

                        return;
                    }
                }

                logger.LogWarning(
                    "Tenant context not found for host: {Host}",
                    host);
            }
            catch (OperationCanceledException ex)
            {
                logger.LogWarning(
                    ex,
                    "Tenant context resolution was cancelled for host: {Host}",
                    host);

                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Failed to resolve tenant context for host: {Host}",
                    host);

                throw;
            }
        }
        else
        {
            logger.LogWarning("Request host is empty.");
        }

        await next(context);

        logger.LogInformation(
            "Request completed without tenant context. StatusCode: {StatusCode}",
            context.Response.StatusCode);
    }
}