using CourseService.BLL.Interface;
using CourseService.BLL.Service;
using CourseService.BLL.Validators;
using CourseService.DAL.Models;
using CourseService.DAL.Repo;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using CourseService.BLL.UserContext;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

// Swagger
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

// Connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultString");

// DbContext
builder.Services.AddDbContext<CourseContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
builder.Services.AddScoped<IModuleRepository, ModuleRepository>();
builder.Services.AddScoped<ICourseService, CourseServiceimpl>();
builder.Services.AddScoped<IModuleService, ModuleService>();

// HttpClient Registrations
builder.Services.AddHttpClient("UserService", client =>
{
    client.BaseAddress = new Uri("https://localhost:7130");
});

builder.Services.AddHttpClient("AssessmentService", client =>
{
    client.BaseAddress = new Uri("https://localhost:7108");
});
builder.Services.AddHttpClient("ProgressService", client =>
{
    client.BaseAddress = new Uri("https://localhost:7175");
});
builder.Services.AddHttpClient("NotificationService", client =>
{
    client.BaseAddress = new Uri("https://localhost:7245");
});

// Fluent Validation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddControllers();

// AutoMapper
builder.Services.AddAutoMapper(typeof(CourseProfile));

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

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
