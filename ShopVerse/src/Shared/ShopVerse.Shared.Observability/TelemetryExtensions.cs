using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ShopVerse.Shared.Observability
{
    /// <summary>
    /// OpenTelemetry distributed tracing yapılandırmasını merkezi hale getiren extension sınıfıdır.
    /// Her servise ASP.NET Core, HTTP Client, EF Core (SqlClient) trace'leri ve
    /// Runtime metriklerini (GC, thread pool, JIT) otomatik olarak ekler.
    /// Jaeger'a OTLP gRPC üzerinden trace gönderir.
    ///
    /// Kullanım (Program.cs):
    ///   builder.Services.AddShopVerseTelemetry("shopverse-order-api");
    ///
    /// Jaeger endpoint: Otlp__Endpoint ortam değişkeni (docker-compose) ile ayarlanır.
    /// Örnek: Otlp__Endpoint=http://shopverse.jaeger:4317
    /// </summary>
    public static class TelemetryExtensions
    {
        private const string DefaultOtlpEndpoint = "http://localhost:4317";

        public static IServiceCollection AddShopVerseTelemetry(
            this IServiceCollection services,
            string serviceName)
        {
            // 1. OtlpExporterOptions'i config'den (Otlp:Endpoint env var) yapılandır
            services.AddOptions<OtlpExporterOptions>()
                .Configure<IConfiguration>((options, config) =>
                {
                    var endpoint = config.GetValue<string>("Otlp:Endpoint") ?? DefaultOtlpEndpoint;
                    options.Endpoint = new Uri(endpoint);
                });

            // 2. OpenTelemetry TracerProvider + MetricsProvider
            services.AddOpenTelemetry()
                .ConfigureResource(resource => resource
                    .AddService(
                        serviceName: serviceName,
                        serviceVersion: "1.0.0",
                        serviceInstanceId: Environment.MachineName))
                .WithTracing(tracing =>
                {
                    // ASP.NET Core — tüm HTTP incoming request'leri otomatik trace eder
                    tracing.AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                    });

                    // Outgoing HTTP client calls (HttpClient, gRPC stubs)
                    tracing.AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                    });

                    // SQL Client — EF Core sorgularını ve Dapper raw SQL'leri trace eder
                    // db.query.text otomatik olarak yakalanır (v1.16+)
                    tracing.AddSqlClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                    });

                    // MassTransit 8.x OTel otomatik olarak entegre olur.
                    // SDK mevcut olduğunda consumer/publisher span'ları otomatik oluşturulur.

                    // OTLP exporter — OtlpExporterOptions üzerinden Jaeger'a trace gönderir
                    tracing.AddOtlpExporter();
                })
                .WithMetrics(metrics =>
                {
                    // ASP.NET Core HTTP metrikleri (request count, latency)
                    metrics.AddAspNetCoreInstrumentation();

                    // HTTP client metrikleri (outgoing request latency)
                    metrics.AddHttpClientInstrumentation();

                    // .NET Runtime metrikleri (GC, thread pool, JIT)
                    metrics.AddRuntimeInstrumentation();

                    // OTLP exporter for metrics
                    metrics.AddOtlpExporter();
                });

            // .NET 8 built-in ProblemDetailsService for RFC 7807 support
            services.AddProblemDetails();

            return services;
        }
    }
}
