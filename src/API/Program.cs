using API.Middleware;

using Application.Configuration;
using Application.Interfaces;
using Application.Services;
using Application.Validators;

using FluentValidation;

using Infrastructure.Data;
using Infrastructure.Data.Repositories;
using Infrastructure.Services;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using Microsoft.OpenApi.Models;

using System.Text;

var builder = WebApplication.CreateBuilder(args);

// =====================================================
// JWT Configuration
// =====================================================

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection(
        JwtSettings.SectionName));

var jwtSettings = builder.Configuration
    .GetSection(JwtSettings.SectionName)
    .Get<JwtSettings>()
    ?? throw new InvalidOperationException(
        "JWT settings are not configured.");

// =====================================================
// JWT Authentication
// =====================================================

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            jwtSettings.Key)),

                ClockSkew = TimeSpan.Zero
            };
    });

// =====================================================
// Controllers
// =====================================================

builder.Services.AddControllers();

// =====================================================
// Entity Framework Core + SQL Server
// =====================================================

builder.Services.AddDbContext<ApplicationDbContext>(
    options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString(
                "DefaultConnection")));

// =====================================================
// Application Services
// =====================================================

builder.Services.AddScoped<
    IProductService,
    ProductService>();

builder.Services.AddScoped<
    IAuthService,
    AuthService>();

// =====================================================
// Infrastructure Services
// =====================================================

builder.Services.AddScoped<
    IProductRepository,
    ProductRepository>();

builder.Services.AddScoped<
    IUnitOfWork,
    UnitOfWork>();

builder.Services.AddScoped<
    IUserRepository,
    UserRepository>();

builder.Services.AddScoped<
    IRefreshTokenRepository,
    RefreshTokenRepository>();

builder.Services.AddScoped<
    IPasswordHasher,
    PasswordHasher>();

builder.Services.AddScoped<
    IJwtTokenService,
    JwtTokenService>();

// =====================================================
// FluentValidation
// =====================================================

builder.Services.AddValidatorsFromAssemblyContaining<
    CreateProductValidator>();

// =====================================================
// Swagger
// =====================================================

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "Product API",
            Version = "v1",
            Description =
                "Product API with JWT Authentication"
        });

    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",

            Type = SecuritySchemeType.Http,

            Scheme = "Bearer",

            BearerFormat = "JWT",

            In = ParameterLocation.Header,

            Description =
                "Enter your JWT access token."
        });

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference =
                        new OpenApiReference
                        {
                            Type =
                                ReferenceType.SecurityScheme,

                            Id = "Bearer"
                        }
                },

                Array.Empty<string>()
            }
        });
});

// =====================================================
// Build Application
// =====================================================

var app = builder.Build();

// =====================================================
// Global Exception Handling Middleware
// =====================================================

app.UseMiddleware<
    ExceptionHandlingMiddleware>();

// =====================================================
// Swagger
// =====================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}

// =====================================================
// HTTPS
// =====================================================

app.UseHttpsRedirection();

// =====================================================
// Authentication & Authorization
// =====================================================

app.UseAuthentication();

app.UseAuthorization();

// =====================================================
// Controllers
// =====================================================

app.MapControllers();

// =====================================================
// Run
// =====================================================

app.Run();