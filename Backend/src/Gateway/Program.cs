var builder = WebApplication.CreateBuilder(args);

<<<<<<< Updated upstream
// Add services to the container.
=======
// DB and repos
services.AddDbContext<GatewayServiceContext>(opt => opt.UseSqlServer(config.GetConnectionString("DefaultConnection")));
services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
services.AddScoped<IAuthService, AuthService>();
services.AddHttpClient();

>>>>>>> Stashed changes

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
