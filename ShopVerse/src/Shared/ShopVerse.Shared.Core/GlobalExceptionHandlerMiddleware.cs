using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace ShopVerse.Shared.Core
{
    /// <summary>
    /// Uygulamada oluşan tüm hataları tek merkezden yakalayıp RFC 7807 ProblemDetails
    /// formatında HTTP response'a çeviren middleware'dir.
    /// Trace ID (OpenTelemetry Activity) ve Correlation ID'yi de response'a ekler.
    /// </summary>
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public GlobalExceptionHandlerMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while processing {Path}", context.Request.Path);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var statusCode = GetStatusCode(exception);

            // Built-in RFC 7807 ProblemDetails (Microsoft.AspNetCore.Mvc)
            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = GetTitle(statusCode),
                Detail = exception.Message,
                Instance = context.Request.Path,
                Type = $"https://httpstatuses.io/{statusCode}"
            };

            // OpenTelemetry trace ID (Activity.Current.Id → W3C trace format)
            var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
            problemDetails.Extensions["traceId"] = traceId;

            // Correlation ID from middleware (X-Correlation-Id header)
            if (context.Items.TryGetValue("X-Correlation-Id", out var correlationId))
            {
                problemDetails.Extensions["correlationId"] = correlationId?.ToString() ?? string.Empty;
            }

#if DEBUG
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
#endif

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = statusCode;

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(problemDetails, options);
            return context.Response.WriteAsync(json);
        }

        private static int GetStatusCode(Exception exception) =>
            exception switch
            {
                ArgumentNullException => (int)HttpStatusCode.BadRequest,
                KeyNotFoundException => (int)HttpStatusCode.NotFound,
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                ArgumentException => (int)HttpStatusCode.BadRequest,
                InvalidOperationException => (int)HttpStatusCode.BadRequest,
                _ => (int)HttpStatusCode.InternalServerError
            };

        private static string GetTitle(int statusCode) =>
            statusCode switch
            {
                400 => "Bad Request",
                401 => "Unauthorized",
                403 => "Forbidden",
                404 => "Not Found",
                409 => "Conflict",
                _ => "Internal Server Error"
            };
    }
}