using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using UserService.BLL.Interface;
using UserService.BLL.Mapping;
using UserService.BLL.UserContext;
using UserService.BLL.Validators;
using UserService.DAL.Models;
using UserService.DAL.Repo;

var builder = WebApplication.CreateBuilder(args);

// =======================
// Controllers + Swagger
// =======================
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContextAccessor, UserContextAccessor>();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "User Service API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "Enter: Bearer {your access token}"
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

// =======================
// Database
// =======================
builder.Services.AddDbContext<UserContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"))
);

// =======================
// Dependency Injection
// =======================
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserServiceImpl>();

builder.Services.AddAutoMapper(typeof(UserProfile));

// =======================
// FluentValidation
// =======================
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddControllers();

// =======================
// JWT AUTHENTICATION (JWE – SAME AS GATEWAY)
// =======================
var jwtSection = builder.Configuration.GetSection("Jwt");

var signingKey = Encoding.UTF8.GetBytes(jwtSection["Key"]!);
var rawEncKey = Encoding.UTF8.GetBytes(jwtSection["EncryptionKey"]!);
var derivedEncKey = SHA256.HashData(rawEncKey);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;

        // REQUIRED for encrypted JWT (JWE)
        options.SecurityTokenValidators.Clear();
        options.SecurityTokenValidators.Add(new JwtSecurityTokenHandler());

        options.TokenValidationParameters = new TokenValidationParameters
        {
            // SIGNATURE
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(signingKey),

            // ENCRYPTION
            TokenDecryptionKey = new SymmetricSecurityKey(derivedEncKey),

            // ISSUER / AUDIENCE
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],

            ValidateAudience = false, // MUST match Gateway
            ValidAudience = jwtSection["Audience"],

            // LIFETIME
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// =======================
// Build app
// =======================
var app = builder.Build();

// =======================
// HTTP pipeline
// =======================
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
