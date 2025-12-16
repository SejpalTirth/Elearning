using CourseService.BLL.Interface;
using CourseService.BLL.Service;
using CourseService.DAL.Models;
using CourseService.DAL.Repo;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.AspNetCore;
using CourseService.BLL.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
builder.Services.AddControllers()
    .AddFluentValidation(fv =>
    {
        fv.RegisterValidatorsFromAssemblyContaining<CourseDtoValidator>();
    });

// AutoMapper
builder.Services.AddAutoMapper(typeof(CourseProfile));

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
