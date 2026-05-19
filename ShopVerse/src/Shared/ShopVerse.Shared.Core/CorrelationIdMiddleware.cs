using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopVerse.Shared.Core
{
    /// <summary>
    /// Her HTTP isteği için bir Correlation ID (izleme kimliği) oluşturan veya mevcut olanı kullanan middleware'dir.
    /// Bu ID, loglama ve dağıtık sistemlerde (microservice) istek takibini kolaylaştırmak için kullanılır.
    /// </summary>
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private const string CorrelationIdHeader = "X-Correlation-Id";

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 1. Header'dan Correlation ID'yi al, yoksa yeni oluştur
            var correlationId = GetOrGenerateCorrelationId(context);

            // 2. Context'e ekle (diğer middleware/loglama için erişilebilir olması için)
            context.Items[CorrelationIdHeader] = correlationId;

            // 3. Response header'a da ekle (client tarafında takip edebilmesi için)
            context.Response.OnStarting(() =>
            {
                if (!context.Response.Headers.ContainsKey(CorrelationIdHeader))
                {
                    context.Response.Headers.Append(CorrelationIdHeader, correlationId);
                }
                return Task.CompletedTask;
            });

            await _next(context);
        }

        private static string GetOrGenerateCorrelationId(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var existingId) &&
                !string.IsNullOrWhiteSpace(existingId))
            {
                return existingId.ToString();
            }

            return Guid.NewGuid().ToString();
        }
    }
}
