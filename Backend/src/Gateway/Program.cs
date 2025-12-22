using GatewayService.BLL.Interface;
using GatewayService.DAL.Data;
using GatewayService.DAL.Repo;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var config = builder.Configuration;

// ----------------------
// Database
// ----------------------
services.AddDbContext<GatewayServiceContext>(options =>
    options.UseSqlServer(config.GetConnectionString("DefaultConnection"))
);

// ----------------------
// Controllers
// ----------------------
services.AddControllers();
services.AddEndpointsApiExplorer();

// ----------------------
// Swagger (FIXED)
// ----------------------
services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Gateway API",
        Version = "v1"
    });

    // IMPORTANT FIX: Prevent DTO name collisions
    c.CustomSchemaIds(type => type.FullName);

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "Enter: Bearer {encrypted access token}"
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

// ----------------------
// HttpClients
// ----------------------
services.AddHttpClient("CourseService", c =>
    c.BaseAddress = new Uri("https://localhost:7190/"));

services.AddHttpClient("AssessmentService", c =>
    c.BaseAddress = new Uri("https://localhost:7108/"));

services.AddHttpClient("NotificationService", c =>
    c.BaseAddress = new Uri("https://localhost:7245/"));

services.AddHttpClient("ProgressService", c =>
    c.BaseAddress = new Uri("https://localhost:7175/"));

services.AddHttpClient("UserService", c =>
    c.BaseAddress = new Uri("https://localhost:7130/"));

// ----------------------
// Repositories + Services
// ----------------------
services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
services.AddScoped<IAuthService, AuthService>();

// ----------------------
// Authentication (Encrypted JWT / JWE)
// ----------------------
services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true;
        options.SaveToken = true;

        // Required for JWE support
        options.SecurityTokenValidators.Clear();
        options.SecurityTokenValidators.Add(new JwtSecurityTokenHandler());

        var rawEncKey = Encoding.UTF8.GetBytes(config["Jwt:EncryptionKey"]!);
        var derivedEncKey = SHA256.HashData(rawEncKey);

        options.TokenValidationParameters = new TokenValidationParameters
        {
            // Signing
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(config["Jwt:Key"]!)
            ),

            // Encryption (JWE)
            TokenDecryptionKey = new SymmetricSecurityKey(derivedEncKey),

            // Issuer / Audience
            ValidateIssuer = true,
            ValidIssuer = config["Jwt:Issuer"],

            ValidateAudience = false,
            ValidAudience = config["Jwt:Audience"],

            // Lifetime
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    })
    // ----------------------
    // Cookies (internal + external)
    // ----------------------
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.Cookie.Name = ".Gateway.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.None;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    })
    .AddCookie("External", options =>
    {
        options.Cookie.Name = ".Gateway.External";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.None;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    })
    // ----------------------
    // External Providers
    // ----------------------
    .AddGoogle(options =>
    {
        options.SignInScheme = "External";
        options.ClientId = config["Authentication:Google:ClientId"];
        options.ClientSecret = config["Authentication:Google:ClientSecret"];
        options.CallbackPath = "/signin-google";
        options.Scope.Add("email");
        options.Scope.Add("profile");
        options.SaveTokens = true;
    })
    .AddMicrosoftAccount(options =>
    {
        options.SignInScheme = "External";
        options.ClientId = config["Authentication:Microsoft:ClientId"];
        options.ClientSecret = config["Authentication:Microsoft:ClientSecret"];
        options.CallbackPath = "/signin-microsoft";
        options.Scope.Add("User.Read");
        options.SaveTokens = true;
    });

// ----------------------
// Authorization
// ----------------------
services.AddAuthorization();

// ----------------------
// CORS
// ----------------------
services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ----------------------
// Build
// ----------------------
var app = builder.Build();

// ----------------------
// Pipeline
// ----------------------
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("AllowAngular");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
