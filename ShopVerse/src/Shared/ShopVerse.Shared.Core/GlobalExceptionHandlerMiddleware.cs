using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http;
using System.Text.Json;

namespace ShopVerse.Shared.Core;
// Uygulamada oluşan tüm hataları tek merkezden yakalayıp HTTP response’a çeviren yapı.
// Uygulamadaki tüm exception’ları yakalar → loglar → HTTP problem response formatına çevirir.
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
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
            _logger.LogError(ex, "An unexpected error occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = GetStatusCode(exception);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = GetTitle(statusCode),
            Detail = exception.Message,
            Instance = context.Request.Path
        };

        // Geliştirme ortamında stack trace eklemek isterseniz:
#if DEBUG
        problemDetails.Extensions["trace"] = exception.StackTrace;
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
            _ => (int)HttpStatusCode.InternalServerError
        };

    private static string GetTitle(int statusCode) =>
        statusCode switch
        {
            400 => "Bad Request",
            401 => "Unauthorized",
            404 => "Not Found",
            _ => "Internal Server Error"
        };
}

// Basit ProblemDetails sınıfı (System.Web.Http.ProblemDetails yerine kendi tanımımız veya .NET 6+ built-in kullanılabilir)
public class ProblemDetails
{
    public string Type { get; set; } = "about:blank";
    public string Title { get; set; } = string.Empty; // Null yerine boş string
    public int? Status { get; set; }
    public string Detail { get; set; } = string.Empty; // Null yerine boş string
    public string Instance { get; set; } = string.Empty; // Null yerine boş string
    public Dictionary<string, object> Extensions { get; set; } = new();
}