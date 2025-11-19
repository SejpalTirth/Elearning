using AssessmentService.DAL;
using AssessmentService.DAL.Repo;
using AssessmentService.BLL.Interfaces;
using AssessmentService.BLL.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Get connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Register DbContext
builder.Services.AddDbContext<AssessmentDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register Repositories (DAL)
builder.Services.AddScoped<IQuizRepository, QuizRepository>();
builder.Services.AddScoped<ISubmissionRepository, SubmissionRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();

// Register BLL Services
builder.Services.AddScoped<IAssessmentService, AssessmentServiceImpl>();

var app = builder.Build();

// Configure HTTP pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
