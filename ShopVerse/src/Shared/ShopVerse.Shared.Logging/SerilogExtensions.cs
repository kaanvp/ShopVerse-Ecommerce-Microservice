using Microsoft.Extensions.Hosting;
using System;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Seq;
using Microsoft.Extensions.Configuration;

namespace ShopVerse.Shared.Logging
{
    /// <summary>
    /// Serilog yapılandırmasını merkezi hale getiren extension sınıfıdır.
    /// Uygulama genelinde logging ayarlarını (log seviyesi, enrichers ve sink'ler) standartlaştırır.
    /// Console ve Seq loglama altyapısını kurar ve Host builder üzerinden kolay entegrasyon sağlar.
    /// </summary>
    public static class SerilogExtensions
    {
        public static IHostBuilder UseSharedLogging(this IHostBuilder builder)
        {
            return builder.UseSerilog((context, services, configuration) =>
            {
                // 1. Minimum Log Level Ayarı
                configuration.MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .MinimumLevel.Override("System", LogEventLevel.Warning);

                // 2. Enrichers (Zenginleştiriciler)
                configuration.Enrich.FromLogContext();

                // Correlation ID'yi context'ten alıp loga ekleme
                configuration.Enrich.WithProperty("ApplicationName", "ShopVerse");

                // Not: CorrelationIdMiddleware tarafından HttpContext.Items'a eklenen ID'yi 
                // loglara otomatik eklemek için özel bir enricher veya middleware entegrasyonu gerekebilir.
                // Basit çözüm: Middleware'de LogContext.PushProperty kullanımı veya aşağıdaki gibi 
                // HTTP Context accessor üzerinden erişim (daha karmaşık). 
                // Şimdilik standart FromLogContext yeterli, detaylı correlation için OpenTelemetry/Sprint 9 planlanmıştır.

                // 3. Sinks (Çıktılar)
                configuration.WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} <s:{SourceContext}>{NewLine}{Exception}"
                );

                var seqUrl = context.Configuration.GetValue<string>("Seq:ServerUrl") ?? "http://localhost:5341";

                configuration.WriteTo.Seq(
                    serverUrl: seqUrl,
                    apiKey: context.Configuration.GetValue<string>("Seq:ApiKey"),
                    restrictedToMinimumLevel: LogEventLevel.Information
                );
            });
        }
    }
}
