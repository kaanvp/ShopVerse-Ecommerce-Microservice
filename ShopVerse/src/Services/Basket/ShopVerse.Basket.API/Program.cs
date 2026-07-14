using Serilog;
using MediatR;
using ShopVerse.Shared.Logging;
using ShopVerse.Shared.Core;
using ShopVerse.Basket.Infrastructure.Services;
using ShopVerse.Basket.Application.Interfaces;
using Grpc.Net.Client;
using StackExchange.Redis;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

// gRPC: HTTP/2 without TLS (development)
AppContext.SetSwitch("System.Net.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

// Serilog entegrasyonu
builder.Host.UseSharedLogging();

// MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(ShopVerse.Basket.Application.Commands.AddToBasketCommand.AddToBasketCommand).Assembly));

// Redis
var redisConn = builder.Configuration.GetValue<string>("Redis:ConnectionString") ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(redisConn));

// Basket Repository
builder.Services.AddScoped<IBasketRepository, BasketRepository>();

// gRPC Channel - Catalog servisine baglan
builder.Services.AddSingleton(sp =>
{
    var catalogUrl = builder.Configuration.GetValue<string>("GrpcSettings:CatalogUrl") ?? "http://shopverse.catalog.api:5000";
    return GrpcChannel.ForAddress(catalogUrl);
});

// gRPC Client
builder.Services.AddScoped<ICatalogGrpcClient, CatalogGrpcClient>();

// HttpContextAccessor (user ID icin)
builder.Services.AddHttpContextAccessor();

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
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Basket API", Version = "v1" });
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
