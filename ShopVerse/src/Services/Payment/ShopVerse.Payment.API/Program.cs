// ShopVerse.Payment.API\Program.cs
using Serilog;
using ShopVerse.Shared.Logging;
using ShopVerse.Shared.Core;
using ShopVerse.Shared.Observability;
using ShopVerse.Payment.Infrastructure.Consumers;
using ShopVerse.Payment.Infrastructure.Data;
using ShopVerse.Payment.Infrastructure.Data.Repositories;
using ShopVerse.Payment.Infrastructure.Jobs;
using ShopVerse.Payment.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using MassTransit;
using Hangfire;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSharedLogging();

// DbContext — MSSQL (Order ile aynı instance)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string not found.");
builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseSqlServer(connectionString));

// Repositories
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// MassTransit + RabbitMQ
builder.Services.AddMassTransit(config =>
{
    config.AddConsumer<ProcessPaymentConsumer>();

    config.UsingRabbitMq((context, cfg) =>
    {
        var host = builder.Configuration.GetValue<string>("RabbitMQ:Host") ?? "localhost";
        cfg.Host(host, "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ReceiveEndpoint("payment-process-queue", e =>
        {
            e.ConfigureConsumer<ProcessPaymentConsumer>(context);
        });
    });
});

// Hangfire
builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage(connectionString));
builder.Services.AddHangfireServer();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Payment API", Version = "v1" });
});

// OpenTelemetry distributed tracing + ProblemDetails (RFC 7807)
builder.Services.AddShopVerseTelemetry("shopverse-payment-api");

var app = builder.Build();

// Hangfire dashboard (opsiyonel)
app.UseHangfireDashboard("/hangfire");

// Recurring job — 1 dakikada bir çalışır
RecurringJob.AddOrUpdate<PaymentTimeoutJob>(
    "payment-timeout-job",
    job => job.ProcessExpiredPayments(),
    "*/1 * * * *"); // her 1 dk'da

app.UseSerilogRequestLogging();
app.UseMiddleware<CorrelationIdMiddleware>();

// Global exception handler — RFC 7807 ProblemDetails
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();