using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using ShopVerse.Cargo.Application.Commands.UpdateShipmentStatus;
using ShopVerse.Cargo.Application.Interfaces;
using ShopVerse.Cargo.Infrastructure.Consumers;
using ShopVerse.Cargo.Infrastructure.Data;
using ShopVerse.Shared.Core;
using ShopVerse.Shared.Logging;
using ShopVerse.Shared.Observability;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSharedLogging();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("PostgreSQL connection string not found.");

// Ensure table exists on startup (Dapper / raw SQL — no migrations)
await DatabaseInitializer.InitializeAsync(connectionString);

// Repositories
builder.Services.AddScoped<IShipmentRepository>(_ => new ShipmentRepository(connectionString));

// MediatR — scan Application assembly
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(UpdateShipmentStatusCommand).Assembly));

// HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// MassTransit + RabbitMQ
builder.Services.AddMassTransit(config =>
{
    config.AddConsumer<CreateShipmentConsumer>();

    config.UsingRabbitMq((context, cfg) =>
    {
        var host = builder.Configuration.GetValue<string>("RabbitMQ:Host") ?? "localhost";
        cfg.Host(host, "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ReceiveEndpoint("cargo-create-shipment-queue", e =>
        {
            e.ConfigureConsumer<CreateShipmentConsumer>(context);
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
});

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Cargo API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
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
builder.Services.AddShopVerseTelemetry("shopverse-cargo-api");

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

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
