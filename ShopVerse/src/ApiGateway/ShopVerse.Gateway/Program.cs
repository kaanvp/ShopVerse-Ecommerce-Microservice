using Serilog;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ShopVerse.Shared.Core;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using ShopVerse.Shared.Logging;
using ShopVerse.Shared.Observability;

var builder = WebApplication.CreateBuilder(args);

// Serilog entegrasyonu
builder.Host.UseSharedLogging();

// ──────────────────────────────────────────────
// 1. JWT Authentication
// ──────────────────────────────────────────────
var jwtSecret = builder.Configuration["Jwt:Secret"]!;
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
    };
});

builder.Services.AddAuthorization();

// ──────────────────────────────────────────────
// 2. CORS
// ──────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ──────────────────────────────────────────────
// 3. Rate Limiting (built-in .NET 7+)
// ──────────────────────────────────────────────
var rateLimitConfig = builder.Configuration.GetSection("RateLimiting");
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter("FixedWindow", opt =>
    {
        opt.PermitLimit = rateLimitConfig.GetValue<int>("PermitLimit", 100);
        opt.Window = TimeSpan.FromSeconds(rateLimitConfig.GetValue<int>("WindowInSeconds", 60));
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = rateLimitConfig.GetValue<int>("QueueLimit", 2);
    });
});

// ──────────────────────────────────────────────
// 4. YARP Reverse Proxy
// ──────────────────────────────────────────────
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// ──────────────────────────────────────────────
// 5. Swagger UI (Aggregated from all services)
// ──────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// OpenTelemetry distributed tracing + ProblemDetails (RFC 7807)
builder.Services.AddShopVerseTelemetry("shopverse-gateway");

var app = builder.Build();

// ──────────────────────────────────────────────
// Middleware Pipeline
// ──────────────────────────────────────────────

// Swagger UI — aggregated view of all downstream services
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/identity/swagger/v1/swagger.json", "Identity API v1");
    c.SwaggerEndpoint("/swagger/catalog/swagger/v1/swagger.json", "Catalog API v1");
    c.SwaggerEndpoint("/swagger/basket/swagger/v1/swagger.json", "Basket API v1");
    c.SwaggerEndpoint("/swagger/order/swagger/v1/swagger.json", "Order API v1");
    c.SwaggerEndpoint("/swagger/payment/swagger/v1/swagger.json", "Payment API v1");
    c.SwaggerEndpoint("/swagger/notification/swagger/v1/swagger.json", "Notification API v1");
    c.SwaggerEndpoint("/swagger/cargo/swagger/v1/swagger.json", "Cargo API v1");
});

// HTTP isteklerini/yanıtlarını Serilog ile logla
app.UseSerilogRequestLogging();

// Correlation ID middleware (istek takibi)
app.UseMiddleware<CorrelationIdMiddleware>();

// Global exception handler — RFC 7807 ProblemDetails
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// Rate Limiter (runs first — rejects early)
app.UseRateLimiter();

// CORS
app.UseCors("AllowAll");

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// YARP Reverse Proxy
app.MapReverseProxy();

app.Run();
