using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using ProgresService.DAL.Data;
using ProgresService.DAL.Repo;
using ProgressService.BLL.Validators;
using ProgressService.DAL.Repo;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using ProgresService.BLL.UserContext;
using ProgresService.BLL.Service;
using ProgresService.BLL.Interface;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContextAccessor, UserContextAccessor>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Assessment Service API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Name = "Authorization",
        Description = "Enter: Bearer {your access token}"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Register DbContext (REQUIRED)
builder.Services.AddDbContext<ProgressDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Repository + Service
builder.Services.AddScoped<IProgressRepository, ProgressRepository>();
builder.Services.AddScoped<IProgressService, ProgressServiceImpl>();
builder.Services.AddHttpClient("UserService", c =>
{
    c.BaseAddress = new Uri("https://localhost:7130/");
});

builder.Services.AddHttpClient("NotificationService", c =>
{
    c.BaseAddress = new Uri("https://localhost:7245/");
});
builder.Services.AddHttpClient("CourseService", c =>
{
    c.BaseAddress = new Uri("https://localhost:7190/");
});

//Fluent Validation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddControllers();

// JWT Bearer Authentication
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

        // REQUIRED for JWE
        options.SecurityTokenValidators.Clear();
        options.SecurityTokenValidators.Add(new JwtSecurityTokenHandler());

        options.TokenValidationParameters = new TokenValidationParameters
        {
            // SIGNATURE VALIDATION
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(signingKey),

            // ENCRYPTION (THIS IS THE KEY FIX)
            TokenDecryptionKey = new SymmetricSecurityKey(derivedEncKey),

            // ISSUER / AUDIENCE
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],

            ValidateAudience = false, // must match Gateway
            ValidAudience = jwtSection["Audience"],

            // LIFETIME
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

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
