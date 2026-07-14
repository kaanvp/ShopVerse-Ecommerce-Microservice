using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using ShopVerse.Order.Application.Commands.CreateOrderCommand;
using ShopVerse.Order.Application.Interfaces;
using ShopVerse.Order.Infrastructure.Data;
using ShopVerse.Order.Infrastructure.Data.Repositories;
using ShopVerse.Order.Infrastructure.Sagas;
using ShopVerse.Order.Infrastructure.Services;
using ShopVerse.Shared.Core;
using ShopVerse.Shared.Logging;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Serilog entegrasyonu
builder.Host.UseSharedLogging();

// EF Core - MSSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(connectionString));

// Saga state DbContext (ayrı tablo için ayrı context)
builder.Services.AddDbContext<OrderStateDbContext>(options =>
    options.UseSqlServer(connectionString));

// MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateOrderCommand).Assembly));

// Repositories
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// HttpContextAccessor (user ID icin)
builder.Services.AddHttpContextAccessor();

// MassTransit + RabbitMQ
builder.Services.AddMassTransit(config =>
{
    // Saga state machine
    config.AddSagaStateMachine<OrderStateMachine, OrderState>()
        .EntityFrameworkRepository(r =>
        {
            r.ConcurrencyMode = ConcurrencyMode.Optimistic;
            r.ExistingDbContext<OrderStateDbContext>();
        });

    config.UsingRabbitMq((context, cfg) =>
    {
        var host = builder.Configuration.GetValue<string>("RabbitMQ:Host") ?? "localhost";
        cfg.Host(host, "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ConfigureEndpoints(context);
    });
});

// Outbox background service
builder.Services.AddHostedService<OutboxProcessor>();

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
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Order API", Version = "v1" });
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

var app = builder.Build();

// HTTP isteklerini/yanıtlarını Serilog ile logla
app.UseSerilogRequestLogging();

// Correlation ID middleware (istek takibi)
app.UseMiddleware<CorrelationIdMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Auto-migration (development)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var orderDb = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    var sagaDb = scope.ServiceProvider.GetRequiredService<OrderStateDbContext>();
    orderDb.Database.Migrate();
    sagaDb.Database.Migrate();
}

app.Run();
