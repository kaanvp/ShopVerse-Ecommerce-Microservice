using Serilog;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using ShopVerse.Identity.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ShopVerse.Identity.Application.Services;
using ShopVerse.Identity.Domain.Entity;
using ShopVerse.Identity.Infrastructure.Seeder;
using ShopVerse.Identity.Infrastructure.Services;
using ShopVerse.Shared.Logging;
using ShopVerse.Shared.Core;
using ShopVerse.Shared.Observability;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Serilog entegrasyonu
builder.Host.UseSharedLogging();
// Add services to the container.

// 2. DbContext
builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. Identity Core
builder.Services.AddIdentity<AppUser, IdentityRole<Guid>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<IdentityDbContext>()
.AddDefaultTokenProviders();

// 4. JWT Authentication
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

// 5. Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("CustomerOrAdmin", policy => policy.RequireRole("Customer", "Admin"));
});

// 6. Application Services
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddHttpContextAccessor();

// 7. MediatR + FluentValidation
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(
    typeof(ShopVerse.Identity.Application.Commands.Login.LoginCommand).Assembly));
builder.Services.AddValidatorsFromAssembly(
    typeof(ShopVerse.Identity.Application.Commands.Login.LoginCommandValidator).Assembly);

// 8. Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Identity API", Version = "v1" });
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

builder.Services.AddShopVerseTelemetry("shopverse-identity-api");

var app = builder.Build();

// HTTP isteklerini/yanıtlarını Serilog ile logla
app.UseSerilogRequestLogging();

// Correlation ID middleware (istek takibi)
app.UseMiddleware<CorrelationIdMiddleware>();

// Global exception handler — RFC 7807 ProblemDetails
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// Veritabanı migrasyonlarını uygula
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    dbContext.Database.Migrate();

    // Seed işlemini çalıştır
    await IdentityDataSeeder.SeedAsync(scope.ServiceProvider);
}

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

app.Run();

