using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ServiceDiscovery;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using ServiceDefaults.HealthChecks;

namespace Microsoft.Extensions.Hosting;

// Adds common Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
// This project should be referenced by each service project in your solution.
// To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults
public static class HostApplicationBuilderExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder AddServiceDefaults()
        {
            builder.ConfigureOpenTelemetry();

            builder.AddDefaultHealthChecks();

            builder.Services.AddServiceDiscovery();

            builder.Services.ConfigureHttpClientDefaults(http =>
            {
                // Turn on resilience by default
                http.AddStandardResilienceHandler();

                // Turn on service discovery by default
                http.AddServiceDiscovery();
            });

            // Restrict the allowed schemes for service discovery
            builder.Services.Configure<ServiceDiscoveryOptions>(options =>
            {
                options.AllowedSchemes = ["https"];
            });

            return builder;
        }

        public IHostApplicationBuilder ConfigureOpenTelemetry()
        {
            builder.Logging.AddOpenTelemetry(logging =>
            {
                logging.IncludeFormattedMessage = true;
                logging.IncludeScopes = true;
            });

            builder.Services.AddOpenTelemetry()
                .WithMetrics(metrics =>
                {
                    metrics.AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddRuntimeInstrumentation();
                })
                .WithTracing(tracing =>
                {
                    tracing.AddSource(builder.Environment.ApplicationName)
                        .AddAspNetCoreInstrumentation(tracing =>
                            // Exclude health check requests from tracing
                            tracing.Filter = context =>
                                !context.Request.Path.StartsWithSegments(HealthCheckPatterns.Health, StringComparison.OrdinalIgnoreCase)
                                && !context.Request.Path.StartsWithSegments(HealthCheckPatterns.Aliveness, StringComparison.OrdinalIgnoreCase)
                        )
                        .AddHttpClientInstrumentation();
                });

            builder.AddOpenTelemetryExporters();

            return builder;
        }

        private IHostApplicationBuilder AddOpenTelemetryExporters()
        {
            // OpenTelemetry Protocol (OTLP) exporter
            var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);
            if (useOtlpExporter)
            {
                builder.Services.AddOpenTelemetry().UseOtlpExporter();
            }

            // Azure Monitor exporter
            var useAzureMonitorExporter = !string.IsNullOrWhiteSpace(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]);
            if (useAzureMonitorExporter)
            {
                builder.Services.AddOpenTelemetry().UseAzureMonitor();
            }

            return builder;
        }

        public IHostApplicationBuilder AddDefaultHealthChecks()
        {
            builder.Services.AddHealthChecks()
                // Add a default liveness check to ensure app is responsive
                .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

            return builder;
        }
    }
}
