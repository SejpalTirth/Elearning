using GatewayService.BLL.Interface;
using GatewayService.BLL.Security;
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

// Database

services.AddDbContext<GatewayServiceContext>(options =>
    options.UseSqlServer(config.GetConnectionString("DefaultConnection"))
);

// Controllers

services.AddControllers();
services.AddEndpointsApiExplorer();

// Swagger

services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Gateway API",
        Version = "v1"
    });

    c.CustomSchemaIds(type => type.FullName);

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "Bearer {access token}"
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

// HttpClients

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

// Repositories + Services

services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
services.AddScoped<IAuthService, AuthService>();
services.AddScoped<IPasswordDecryptor, PasswordDecryptor>();
services.AddScoped<IPasswordHasher, PasswordHasher>();

// Authentication

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

        options.SecurityTokenValidators.Clear();
        options.SecurityTokenValidators.Add(new JwtSecurityTokenHandler());

        var rawEncKey = Encoding.UTF8.GetBytes(config["Jwt:EncryptionKey"]!);
        var derivedEncKey = SHA256.HashData(rawEncKey);

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(config["Jwt:Key"]!)
            ),

            TokenDecryptionKey = new SymmetricSecurityKey(derivedEncKey),

            ValidateIssuer = true,
            ValidIssuer = config["Jwt:Issuer"],

            ValidateAudience = false,
            ValidateLifetime = true,

            ClockSkew = TimeSpan.FromSeconds(30)
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Cookies["access_token"];
                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            },

            OnTokenValidated = async context =>
            {
                if (!context.HttpContext.Request.Cookies.ContainsKey("access_token"))
                    return;

                DateTime expiresAt;

                if (context.SecurityToken is JwtSecurityToken jwt)
                    expiresAt = jwt.ValidTo;
                else
                    return;

                var remaining = expiresAt - DateTime.UtcNow;
                if (remaining > TimeSpan.FromSeconds(30))
                    return;

                var refreshToken = context.HttpContext.Request.Cookies["refresh_token"];
                if (string.IsNullOrEmpty(refreshToken))
                    return;

                var authService = context.HttpContext.RequestServices
                    .GetRequiredService<IAuthService>();

                var tokens = await authService.RefreshTokenAsync(refreshToken);
                if (tokens == null)
                    return;

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Lax,
                    Path = "/"
                };

                context.HttpContext.Response.Cookies.Append(
                    "access_token",
                    tokens.AccessToken,
                    cookieOptions
                );

                context.HttpContext.Response.Cookies.Append(
                    "refresh_token",
                    tokens.RefreshToken,
                    cookieOptions
                );
            }
        };
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie("External", options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.None;
    })
    .AddGoogle(options =>
    {
        options.SignInScheme = "External";
        options.ClientId = config["Authentication:Google:ClientId"]!;
        options.ClientSecret = config["Authentication:Google:ClientSecret"]!;
    })
    .AddMicrosoftAccount(options =>
    {
        options.SignInScheme = "External";
        options.ClientId = config["Authentication:Microsoft:ClientId"]!;
        options.ClientSecret = config["Authentication:Microsoft:ClientSecret"]!;
    });

// Authorization
services.AddAuthorization();

// CORS (IMPORTANT)
services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Build App
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
