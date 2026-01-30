using AssessmentService.BLL.Interfaces;
using AssessmentService.BLL.Services;
using AssessmentService.BLL.UserContext;
using AssessmentService.BLL.Validators;
using AssessmentService.DAL;
using AssessmentService.DAL.Repo;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// =======================
// Controllers & Swagger
// =======================
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


// =======================
// Database
// =======================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AssessmentDbContext>(options =>
    options.UseSqlServer(connectionString));

// =======================
// Http Clients
// =======================
builder.Services.AddHttpClient("CourseService", c =>
{
    c.BaseAddress = new Uri("https://localhost:7190/");
    c.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddHttpContextAccessor();


// =======================
// Repositories
// =======================
builder.Services.AddScoped<IQuizRepository, QuizRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<ISubmissionRepository, SubmissionRepository>();

// =======================
// Business Services
// =======================
builder.Services.AddScoped<IAssessmentService, AssessmentServiceImpl>();

// =======================
// FluentValidation
// =======================
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddControllers();

// =======================
// AutoMapper
// =======================
builder.Services.AddAutoMapper(typeof(AssessmentProfile));

// =======================
// JWT AUTHENTICATION (JWE COMPATIBLE)
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

// ORDER MATTERS
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
