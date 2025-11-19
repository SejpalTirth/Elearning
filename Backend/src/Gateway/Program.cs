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


services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();


services.AddDbContext<GatewayServiceContext>(opt =>
    opt.UseSqlServer(config.GetConnectionString("DefaultConnection")));


services.AddHttpClient();

services.AddHttpClient("AssessmentService", client =>
{
    client.BaseAddress = new Uri("https://localhost:7108");
});

services.AddHttpClient("NotificationService", client =>
{
    client.BaseAddress = new Uri("https://localhost:7245");
});

services.AddHttpClient("ProgressService", client =>
{
    client.BaseAddress = new Uri("https://localhost:7175");
});

services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
services.AddScoped<IAuthService, AuthService>();


services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
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
        ValidIssuer = config["Jwt:Issuer"],

        ValidateAudience = true,
        ValidAudience = config["Jwt:Audience"],

        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(config["Jwt:Key"])
        ),

        ValidateLifetime = true
    };

    // Debugging support
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
    options.ClientId = config["Authentication:Google:ClientId"];
    options.ClientSecret = config["Authentication:Google:ClientSecret"];
    options.CallbackPath = "/signin-google";
    options.SaveTokens = true;

    options.Scope.Add("email");
    options.Scope.Add("profile");

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
    options.ClientId = config["Authentication:Microsoft:ClientId"];
    options.ClientSecret = config["Authentication:Microsoft:ClientSecret"];
    options.CallbackPath = "/signin-microsoft";
    options.SaveTokens = true;

    options.Scope.Add("User.Read");

    options.Events.OnRemoteFailure = ctx =>
    {
        Console.WriteLine($"Microsoft auth error: {ctx.Failure?.Message}");
        ctx.Response.Redirect("/auth/error");
        ctx.HandleResponse();
        return Task.CompletedTask;
    };
});


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
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
