using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace IdentityService.Application.Common.Extensions;

public static class OpenTelemetryExtensions
{
    public static WebApplicationBuilder AddFinTrackOpenTelemetry(this WebApplicationBuilder builder)
    {
        var otelSection = builder.Configuration.GetSection("OpenTelemetry");
        var serviceName = otelSection["ServiceName"] ?? builder.Environment.ApplicationName;
        var otlpEndpoint = otelSection["OtlpEndpoint"] ?? "http://localhost:4317";

        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(serviceName);

        builder.Logging.AddOpenTelemetry(options =>
        {
            options.IncludeFormattedMessage = true;
            options.IncludeScopes = true;
            options.ParseStateValues = true;
            options.SetResourceBuilder(resourceBuilder);

            options.AddOtlpExporter(otlp =>
            {
                otlp.Endpoint = new Uri(otlpEndpoint);
                otlp.Protocol = OtlpExportProtocol.Grpc;
            });
        });

        builder.Services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                    })
                    .AddHttpClientInstrumentation()
                    .AddSqlClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                    })
                    .AddOtlpExporter(otlp =>
                    {
                        otlp.Endpoint = new Uri(otlpEndpoint);
                        otlp.Protocol = OtlpExportProtocol.Grpc;
                    });
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddProcessInstrumentation()
                    .AddOtlpExporter(otlp =>
                    {
                        otlp.Endpoint = new Uri(otlpEndpoint);
                        otlp.Protocol = OtlpExportProtocol.Grpc;
                    });
            });

        return builder;
    }
}