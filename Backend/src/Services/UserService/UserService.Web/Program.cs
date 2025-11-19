using Microsoft.EntityFrameworkCore;
using UserService.BLL.Interface;
using UserService.BLL.Mapping;
using UserService.BLL.Service;
using UserService.DAL.Models;
using UserService.DAL.Repo;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Connection string
builder.Services.AddDbContext<UserContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// DI
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserServiceImpl>();
builder.Services.AddScoped<IUserAuthService, UserAuthService>();

builder.Services.AddAutoMapper(typeof(UserProfile));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// You MISSED THIS — this actually enables CORS
app.UseCors("AllowAngular");

app.UseAuthorization();

app.MapControllers();

app.Run();
