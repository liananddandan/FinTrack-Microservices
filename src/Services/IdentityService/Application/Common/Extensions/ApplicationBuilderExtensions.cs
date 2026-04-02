using IdentityService.Api.Common.Middlewares;

namespace IdentityService.Application.Common.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseFinTrackTelemetry(this IApplicationBuilder app)
    {
        app.UseMiddleware<TenantTelemetryMiddleware>();
        return app;
    }
}