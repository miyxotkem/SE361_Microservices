using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System;

namespace BuildingBlocks.Diagnostics
{
    public static class OpenTelemetryExtensions
    {
        public static IServiceCollection AddOpenTelemetryDiagnostics(this IServiceCollection services, IConfiguration configuration, string serviceName)
        {
            var otlpEndpoint = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] ?? "http://localhost:4317";
            
            // Read sampling ratio from config (default to 0.20 / 20%)
            var samplingRatio = double.TryParse(configuration["OTEL_TRACE_SAMPLING_RATIO"], out var ratio) ? ratio : 0.2;

            services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService(serviceName))
                .WithTracing(tracing =>
                {
                    tracing
                        .SetSampler(new TraceIdRatioBasedSampler(samplingRatio)) // Configure sampling
                        .AddSource("MassTransit")
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddOtlpExporter(options =>
                        {
                            options.Endpoint = new Uri(otlpEndpoint);
                        });
                })
                .WithMetrics(metrics =>
                {
                    metrics
                        .AddAspNetCoreInstrumentation() // HTTP incoming request metrics
                        .AddHttpClientInstrumentation() // HTTP outgoing request metrics
                        .AddOtlpExporter(options =>
                        {
                            options.Endpoint = new Uri(otlpEndpoint);
                        });
                });

            return services;
        }

        // Backward compatibility mapping
        public static IServiceCollection AddOpenTelemetryTracing(this IServiceCollection services, IConfiguration configuration, string serviceName)
        {
            return services.AddOpenTelemetryDiagnostics(configuration, serviceName);
        }

        public static ILoggingBuilder AddOpenTelemetryLogging(this ILoggingBuilder logging, IConfiguration configuration)
        {
            var otlpEndpoint = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] ?? "http://localhost:4317";

            logging.AddOpenTelemetry(options =>
            {
                options.IncludeFormattedMessage = true;
                options.IncludeScopes = true;
                options.AddOtlpExporter(exporter =>
                {
                    exporter.Endpoint = new Uri(otlpEndpoint);
                });
            });

            return logging;
        }
    }
}

