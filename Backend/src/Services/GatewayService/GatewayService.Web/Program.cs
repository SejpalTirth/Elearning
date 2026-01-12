using GatewayService.BLL.Interface;
using GatewayService.BLL.Security;
using GatewayService.DAL.Data;
using GatewayService.DAL.Repo;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultString");

// DbContext
builder.Services.AddDbContext<GatewayServiceContext>(options =>
    options.UseSqlServer(connectionString));

// DI for Repositories + Services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPasswordDecryptor, CryptoJsPasswordDecryptor>();
builder.Services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
