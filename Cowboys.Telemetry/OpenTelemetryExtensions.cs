using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Cowboys.Telemetry;

public static class OpenTelemetryExtensions
{
    public static void AddOpenTelemetryTraces(this IServiceCollection services, IConfiguration configuration)
    {
        var serviceName = configuration["ServiceName"];

        services.AddOpenTelemetry()
            .ConfigureResource(resourceBuilder =>
            {
                resourceBuilder
                    .AddService(serviceName)
                    .AddTelemetrySdk()
                    .AddEnvironmentVariableDetector();
            })
            .WithTracing(b =>
            {
                b.AddSource("MassTransit")
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation();

                var jaegerHost = configuration["Jaeger:Host"];
                b.AddJaegerExporter(config =>
                {
                    config.AgentHost = jaegerHost ?? "localhost";
                    config.AgentPort = 6831;
                    config.MaxPayloadSizeInBytes = 4096;
                    config.ExportProcessorType = ExportProcessorType.Batch;
                });
                b.AddConsoleExporter();
            }).StartWithHost();
    }
}