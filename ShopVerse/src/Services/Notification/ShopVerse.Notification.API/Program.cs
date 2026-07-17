using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using Serilog;
using ShopVerse.Notification.Application.Interfaces;
using ShopVerse.Notification.Infrastructure.Consumers;
using ShopVerse.Notification.Infrastructure.Data;
using ShopVerse.Notification.Infrastructure.Hubs;
using ShopVerse.Notification.Infrastructure.Settings;
using ShopVerse.Shared.Core;
using ShopVerse.Shared.Logging;
using ShopVerse.Shared.Observability;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Serilog entegrasyonu
builder.Host.UseSharedLogging();

// MongoDbSettings
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

// MongoDB — Client (Singleton)
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = builder.Configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>();
    return new MongoClient(settings!.ConnectionString);
});

// MongoDB — Database (Scoped)
builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var settings = builder.Configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>();
    return client.GetDatabase(settings!.DatabaseName);
});

// Repositories
builder.Services.AddScoped<INotificationRepository>(sp =>
{
    var database = sp.GetRequiredService<IMongoDatabase>();
    var settings = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<MongoDbSettings>>().Value;
    return new NotificationRepository(database, settings);
});
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// SignalR
builder.Services.AddSignalR();

// MassTransit + RabbitMQ
builder.Services.AddMassTransit(config =>
{
    config.AddConsumer<NotificationEventConsumer>();
    config.AddConsumer<OrderCreatedConsumer>();
    config.AddConsumer<PaymentCompletedConsumer>();
    config.AddConsumer<CargoShippedConsumer>();
    config.AddConsumer<CargoStatusUpdatedConsumer>();

    config.UsingRabbitMq((context, cfg) =>
    {
        var host = builder.Configuration.GetValue<string>("RabbitMQ:Host") ?? "localhost";
        cfg.Host(host, "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ReceiveEndpoint("notification-event-queue", e =>
        {
            e.ConfigureConsumer<NotificationEventConsumer>(context);
        });
        cfg.ReceiveEndpoint("notification-order-created-queue", e =>
        {
            e.ConfigureConsumer<OrderCreatedConsumer>(context);
        });
        cfg.ReceiveEndpoint("notification-payment-completed-queue", e =>
        {
            e.ConfigureConsumer<PaymentCompletedConsumer>(context);
        });
        cfg.ReceiveEndpoint("notification-cargo-shipped-queue", e =>
        {
            e.ConfigureConsumer<CargoShippedConsumer>(context);
        });
        cfg.ReceiveEndpoint("notification-cargo-status-updated-queue", e =>
        {
            e.ConfigureConsumer<CargoStatusUpdatedConsumer>(context);
        });
    });
});

// JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "ShopVerse-Super-Secret-Key-2024-MinLength32Chars!";
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

    // SignalR için query string'den token okuma
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/notification"))
                context.Token = accessToken;
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Notification API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT token giriniz"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// OpenTelemetry distributed tracing + ProblemDetails (RFC 7807)
builder.Services.AddShopVerseTelemetry("shopverse-notification-api");

var app = builder.Build();

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
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notification");

app.Run();
