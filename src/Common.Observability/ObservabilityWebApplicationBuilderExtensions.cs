using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Common.Observability;

public static class ObservabilityWebApplicationBuilderExtensions
{
    public static void AddObservability(this WebApplicationBuilder builder, Action<MeterProviderBuilder>? meterProviderBuilder = null, Action<TracerProviderBuilder>? tracerProviderBuilder = null)
    {
        void ConfigureOtlpExporter(OtlpExporterOptions configure)
        {
            var otlpTargetName = builder.Configuration["OtlpTargetName"] ?? string.Empty;
            var otlpTargetUri = builder.Configuration.GetServiceUri(otlpTargetName, "otlp-receiver");
                
            configure.Endpoint = otlpTargetUri;
            configure.BatchExportProcessorOptions.ScheduledDelayMilliseconds = 1000;
            configure.TimeoutMilliseconds = 1000;
        }

        var serviceName = builder.Configuration["ServiceName"] ?? builder.Environment.ApplicationName;
        var serviceVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";

        Tracing.Source = new ActivitySource(serviceName, serviceVersion);

        var resourceBuilder = ResourceBuilder
            .CreateDefault()
            .AddService(
                serviceName,
                serviceVersion: serviceVersion,
                serviceInstanceId: Environment.MachineName);
            
        builder.Services.AddLogging(lb =>
        {
            lb.ClearProviders()
                .SetMinimumLevel(LogLevel.Information)
                .AddOpenTelemetry(options =>
                {
                    options.SetResourceBuilder(resourceBuilder);
                    options.AddOtlpExporter(ConfigureOtlpExporter);
                });
        });

        builder.Services.AddOpenTelemetry()
            .WithTracing(b =>
            {
                b
                    .AddSource(serviceName)
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(ConfigureOtlpExporter);

                tracerProviderBuilder?.Invoke(b);
            })
            .WithMetrics(b =>
            {
                b.SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddPrometheusExporter()
                    .AddOtlpExporter(ConfigureOtlpExporter);

                meterProviderBuilder?.Invoke(b);
            });
    }
}