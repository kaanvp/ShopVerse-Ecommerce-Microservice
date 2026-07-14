using Serilog;
using MediatR;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using MongoDB.Driver;
using StackExchange.Redis;
using ShopVerse.Catalog.Application.Interfaces;
using ShopVerse.Catalog.Domain.Interface;
using ShopVerse.Catalog.Infrastructure.Repositories;
using ShopVerse.Catalog.Infrastructure.Services;
using ShopVerse.Catalog.Infrastructure.Settings;
using ShopVerse.Shared.Core;
using ShopVerse.Shared.Logging;

var builder = WebApplication.CreateBuilder(args);

// Serilog entegrasyonu
builder.Host.UseSharedLogging();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(ShopVerse.Catalog.Application.DTOs.ProductDto).Assembly));

// MongoDbSettings
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

// MongoDB
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = builder.Configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>();
    return new MongoClient(settings!.ConnectionString);
});

builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var settings = builder.Configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>();
    return client.GetDatabase(settings!.DatabaseName);
});

// Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var connectionString = builder.Configuration.GetValue<string>("Redis:ConnectionString")
        ?? "localhost:6379";
    return ConnectionMultiplexer.Connect(connectionString);
});

// Repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IUnitOfWork, MongoUnitOfWork>();

// Services
builder.Services.AddScoped<ICatalogCacheService, CatalogCacheService>();

builder.Services.AddGrpc();

// Kestrel: HTTP (8080) + gRPC (5000)
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1;
    });
    options.ListenAnyIP(5000, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
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

app.UseAuthorization();

app.MapControllers();
app.MapGrpcService<ShopVerse.Catalog.API.Services.CatalogGrpcService>();

app.Run();
