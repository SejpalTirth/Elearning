using GatewayService.BLL.Interface;
using GatewayService.DAL.Data;
using GatewayService.DAL.Repo;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var config = builder.Configuration;

// ----------------------
// Database
// ----------------------
services.AddDbContext<GatewayServiceContext>(opt =>
    opt.UseSqlServer(config.GetConnectionString("DefaultConnection"))
);

// ----------------------
// Controllers + Swagger
// ----------------------
services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

// ----------------------
// Downstream HttpClients
// ----------------------
services.AddHttpClient("CourseService", c =>
{
    c.BaseAddress = new Uri("https://localhost:7190/");
});

services.AddHttpClient("AssessmentService", c =>
{
    c.BaseAddress = new Uri("https://localhost:7108/");
});

services.AddHttpClient("NotificationService", c =>
{
    c.BaseAddress = new Uri("https://localhost:7245/");
});

services.AddHttpClient("ProgressService", c =>
{
    c.BaseAddress = new Uri("https://localhost:7175/");
});

// ----------------------
// Repositories + Services
// ----------------------
services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
services.AddScoped<IAuthService, AuthService>();

// ----------------------
// Authentication
// ----------------------
services
    .AddAuthentication(options =>
    {
        // JWT is default for API
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })

    // Main cookie scheme for internal GatewayAuthController operations
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.Cookie.Name = ".Gateway.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.None;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    })

    // External login cookie (Google/Microsoft)
    .AddCookie("External", options =>
    {
        options.Cookie.Name = ".Gateway.External";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.None;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    })

    // JWT Bearer for API calls
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = config["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = config["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(config["Jwt:Key"])),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    })

    // Google Login
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

    // Microsoft Login
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
