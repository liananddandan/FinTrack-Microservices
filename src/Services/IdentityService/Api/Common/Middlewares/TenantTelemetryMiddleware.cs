using System.Diagnostics;

namespace IdentityService.Api.Common.Middlewares;

public class TenantTelemetryMiddleware(
    RequestDelegate next,
    ILogger<TenantTelemetryMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var host = context.Request.Host.Host.ToLowerInvariant();
        var tenant = ResolveTenant(host);

        Activity.Current?.SetTag("tenant", tenant);
        Activity.Current?.SetTag("host.name", host);

        using (logger.BeginScope(new Dictionary<string, object>
               {
                   ["tenant"] = tenant,
                   ["host"] = host
               }))
        {
            await next(context);
        }
    }

    private static string ResolveTenant(string host)
    {
        if (host.Contains("coffee"))
            return "coffee";

        if (host.Contains("sushi"))
            return "sushi";

        return "unknown";
    }
}