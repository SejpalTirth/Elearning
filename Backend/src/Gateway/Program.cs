using GatewayService.BLL.Interface;
using GatewayService.DAL.Data;
using GatewayService.DAL.Repo;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var config = builder.Configuration;

// DB & Repos
services.AddDbContext<GatewayServiceContext>(opt =>
    opt.UseSqlServer(config.GetConnectionString("DefaultConnection")));

services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
services.AddScoped<IAuthService, AuthService>();

// Authentication
services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddCookie()
.AddCookie("External")
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = config.GetValue<string>("Jwt:Issuer"),

        ValidateAudience = true,
        ValidAudience = config.GetValue<string>("Jwt:Audience"),

        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(config.GetValue<string>("Jwt:Key"))
        ),

        ValidateLifetime = true
    };

    // Added for debuggability
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = ctx =>
        {
            Console.WriteLine($"JWT Auth failed: {ctx.Exception.Message}");
            return Task.CompletedTask;
        }
    };
})
.AddGoogle(options =>
{
    options.SignInScheme = "External";
    options.ClientId = config.GetValue<string>("Authentication:Google:ClientId");
    options.ClientSecret = config.GetValue<string>("Authentication:Google:ClientSecret");
    options.CallbackPath = "/signin-google";
    options.SaveTokens = true;

    options.Scope.Add("email");
    options.Scope.Add("profile");

    // Better error visibility
    options.Events.OnRemoteFailure = ctx =>
    {
        Console.WriteLine($"Google auth error: {ctx.Failure?.Message}");
        ctx.Response.Redirect("/auth/error");
        ctx.HandleResponse();
        return Task.CompletedTask;
    };
})
.AddMicrosoftAccount(options =>
{
    options.SignInScheme = "External";
    options.ClientId = config.GetValue<string>("Authentication:Microsoft:ClientId");
    options.ClientSecret = config.GetValue<string>("Authentication:Microsoft:ClientSecret");
    options.CallbackPath = "/signin-microsoft";
    options.SaveTokens = true;

    options.Scope.Add("User.Read");

    // Better error visibility
    options.Events.OnRemoteFailure = ctx =>
    {
        Console.WriteLine($"Microsoft auth error: {ctx.Failure?.Message}");
        ctx.Response.Redirect("/auth/error");
        ctx.HandleResponse();
        return Task.CompletedTask;
    };
});

// controllers
services.AddControllers();

// Swagger & CORS
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();

app.UseCors(policy => policy
    .WithOrigins("http://localhost:4200")
    .AllowAnyHeader()
    .AllowAnyMethod()
);

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
