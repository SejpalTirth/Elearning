using GatewayService.BLL.Interface;
using GatewayService.DAL.Data;
using GatewayService.DAL.Repo;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var config = builder.Configuration;

// DB and repos
services.AddDbContext<GatewayServiceContext>(opt => opt.UseSqlServer(config.GetConnectionString("DefaultConnection")));
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
        ValidIssuer = config["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = config["Jwt:Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"])),
        ValidateLifetime = true
    };
})
.AddGoogle(options =>
{
    options.SignInScheme = "External";
    options.ClientId = config["Authentication:Google:ClientId"];
    options.ClientSecret = config["Authentication:Google:ClientSecret"];
    options.CallbackPath = "/signin-google"; // default path the middleware handles automatically
    options.SaveTokens = true;
    options.Scope.Add("email");
    options.Scope.Add("profile");
})
.AddMicrosoftAccount(options =>
{
    options.SignInScheme = "External";
    options.ClientId = config["Authentication:Microsoft:ClientId"];
    options.ClientSecret = config["Authentication:Microsoft:ClientSecret"];
    options.CallbackPath = "/signin-microsoft"; // default path
    options.SaveTokens = true;
    options.Scope.Add("User.Read");
});




// controllers
services.AddControllers();

// swagger, cors etc.
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseRouting();
app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
