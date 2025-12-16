using AssessmentService.BLL.DTOs;
using AssessmentService.BLL.Interfaces;
using AssessmentService.DAL.Models;
using AssessmentService.DAL.Repo;
using AutoMapper;
using System.Net.Http.Json;
using System.Text.Json;

namespace AssessmentService.BLL.Services
{
    public class AssessmentServiceImpl : IAssessmentService
    {
        private readonly IQuizRepository _quizRepo;
        private readonly ISubmissionRepository _submissionRepo;
        private readonly IQuestionRepository _questionRepo;
        private readonly IHttpClientFactory _httpFactory;
        private readonly IMapper _mapper;

        private const decimal PASS_PERCENTAGE = 67m;

        public AssessmentServiceImpl(
            IQuizRepository quizRepo,
            ISubmissionRepository submissionRepo,
            IQuestionRepository questionRepo,
            IHttpClientFactory httpFactory,
            IMapper mapper)
        {
            _quizRepo = quizRepo;
            _submissionRepo = submissionRepo;
            _questionRepo = questionRepo;
            _httpFactory = httpFactory;
            _mapper = mapper;
        }

        // CREATE QUIZ
        public async Task<object> CreateQuizAsync(CreateQuizDto dto)
        {
            var existing = await _quizRepo.GetByModuleIdAsync(dto.ModuleId);
            if (existing != null)
                return new { message = "Quiz already exists for this module.", quizId = existing.Id };

            var quiz = new Quiz
            {
                ModuleId = dto.ModuleId,
                Title = dto.Title,
                TimeLimitMinutes = dto.TimeLimitMinutes,
                TotalMarks = 0
            };

            await _quizRepo.AddAsync(quiz);
            await _quizRepo.SaveChangesAsync();

            // AUTO-PUBLISH COURSE OF THIS MODULE
            try
            {
                var client = CreateCourseServiceClient();
                if (client != null)
                {
                    var moduleResponse = await client.GetAsync($"/api/modules/{dto.ModuleId}");
                    if (moduleResponse.IsSuccessStatusCode)
                    {
                        var moduleDetails = await moduleResponse.Content.ReadFromJsonAsync<ModuleDetail>();
                        if (moduleDetails != null)
                        {
                            await client.PostAsync($"/api/courses/{moduleDetails.CourseId}/publish", null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Auto Publish Error] " + ex.Message);
            }

            return new { quiz.Id, quiz.Title, quiz.ModuleId };
        }

        // implemented interface helper methods
        public async Task<IEnumerable<QuizSummaryDto>> GetAllQuizzesAsync()
        {
            var quizzes = await _quizRepo.GetAllAsync();
            return _mapper.Map<IEnumerable<QuizSummaryDto>>(quizzes);
        }

        public async Task<QuizDetailDto?> GetQuizByIdAsync(int id)
        {
            var quiz = await _quizRepo.GetByIdWithDetailsAsync(id);
            return quiz == null ? null : _mapper.Map<QuizDetailDto>(quiz);
        }

        public async Task<QuizResultDto?> GetSubmissionResultAsync(Guid submissionId)
        {
            var submission = await _submissionRepo.GetByIdAsync(submissionId);
            if (submission == null) return null;

            var quiz = await _quizRepo.GetByIdAsync(submission.QuizId);
            if (quiz == null) return null;

            int totalMarks = quiz.TotalMarks ?? quiz.Questions.Sum(q => q.Marks);
            var percentage = Math.Round(
                (decimal)(submission.Score ?? 0) / totalMarks * 100m, 2
            );

            return new QuizResultDto
            {
                TotalMarks = totalMarks,
                ObtainedMarks = submission.Score ?? 0,
                Percentage = percentage,
                Passed = percentage >= PASS_PERCENTAGE,
                AlreadyPassed = true,
                StatusMessage = percentage >= PASS_PERCENTAGE
                    ? "You have passed this quiz."
                    : "Try again to improve your score."
            };
        }

        // ADD QUESTION
        public async Task<object> AddQuestionAsync(int quizId, CreateQuestionDto dto)
        {
            var quiz = await _quizRepo.GetByIdAsync(quizId);
            if (quiz == null) return new { message = "Quiz not found." };

            ValidateQuestion(dto);

            var question = _mapper.Map<Question>(dto);
            question.QuizId = quizId;

            for (int i = 0; i < dto.Options.Count; i++)
            {
                question.Answers.Add(new Answer
                {
                    AnswerText = dto.Options[i],
                    IsCorrect = i == dto.CorrectAnswerIndex
                });
            }

            await _questionRepo.AddAsync(question);

            quiz.TotalMarks = (quiz.TotalMarks ?? 0) + dto.Marks;
            await _quizRepo.SaveChangesAsync();

            return new { message = "Question added successfully.", questionId = question.Id };
        }

        // GET QUIZ FOR MODULE
        public async Task<QuizForModuleDto?> GetQuizForModuleAsync(int moduleId, Guid userId)
        {
            var quiz = await _quizRepo.GetByModuleIdAsync(moduleId);
            if (quiz == null) return null;

            var dto = _mapper.Map<QuizForModuleDto>(quiz);
            dto.ModuleId = moduleId;

            var best = await _submissionRepo.GetBestSubmissionAsync(quiz.Id, userId);
            if (best?.Score != null)
            {
                dto.AlreadyPassed =
                    CalculatePercentage(best.Score.Value, dto.TotalMarks) >= PASS_PERCENTAGE;
            }

            return dto;
        }

        // SUBMIT QUIZ
        public async Task<QuizResultDto> SubmitQuizAsync(SubmitQuizDto dto)
        {
            var quiz = await _quizRepo.GetByIdWithDetailsAsync(dto.QuizId)
                ?? throw new Exception("Quiz not found.");

            int totalMarks = quiz.Questions.Sum(q => q.Marks);
            int totalQuestions = quiz.Questions.Count;

            var previous = await _submissionRepo.GetBestSubmissionAsync(dto.QuizId, dto.UserId);
            if (HasAlreadyPassed(previous, totalMarks))
                return BuildAlreadyPassedResult(previous!, quiz, totalMarks, totalQuestions);

            var (obtained, correct) = EvaluateQuiz(quiz, dto);
            var percentage = CalculatePercentage(obtained, totalMarks);

            var submission = new QuizSubmission
            {
                QuizId = quiz.Id,
                UserId = dto.UserId,
                Score = obtained,
                SubmittedData = JsonSerializer.Serialize(dto.Answers)
            };

            await _submissionRepo.AddAsync(submission);
            await _submissionRepo.SaveChangesAsync();

            return BuildResult(submission, obtained, correct, totalMarks, totalQuestions, percentage);
        }

        // PUBLIC: Get module IDs without quiz
        public async Task<List<int>> GetModulesWithoutQuizByCourseAsync(int courseId)
        {
            var modules = await FetchModulesForCourseAsync(courseId);
            if (modules == null) return new List<int>();

            var moduleIds = modules.Select(m => m.Id).ToList();
            return await GetModulesWithoutQuizAsync(moduleIds);
        }

        // COURSE QUIZ STATUS
        public async Task<object> GetQuizStatusForCourseAsync(int courseId)
        {
            var modules = await FetchModulesForCourseAsync(courseId) ?? new List<ModuleInfo>();
            if (!modules.Any())
                return new { courseId, modules = new List<object>(), allQuizzesCreated = false };

            var moduleIds = modules.Select(m => m.Id).ToList();
            var missing = await GetModulesWithoutQuizAsync(moduleIds);

            var moduleStatus = modules.Select(m => new
            {
                moduleId = m.Id,
                title = m.Title,
                quizExists = !missing.Contains(m.Id)
            }).ToList();

            return new
            {
                courseId,
                allQuizzesCreated = missing.Count == 0,
                modules = moduleStatus,
                nextPendingModuleId = moduleStatus.FirstOrDefault(m => !m.quizExists)?.moduleId
            };
        }

        // INTERNAL FUNCTION: Fetch modules for a course
        private async Task<List<ModuleInfo>?> FetchModulesForCourseAsync(int courseId)
        {
            try
            {
                var client = CreateCourseServiceClient();
                if (client == null) return null;

                var resp = await client.GetAsync($"/api/Modules/course/{courseId}");
                if (!resp.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[FetchModules] CourseService returned {(int)resp.StatusCode}");
                    return null;
                }

                var json = await resp.Content.ReadAsStringAsync();

                var modules = JsonSerializer.Deserialize<List<ModuleInfo>>(json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                return modules ?? new List<ModuleInfo>();
            }
            catch (Exception ex)
            {
                Console.WriteLine("[FetchModules] error: " + ex.Message);
                return null;
            }
        }

        private HttpClient? CreateCourseServiceClient()
        {
            try
            {
                return _httpFactory.CreateClient("CourseService");
            }
            catch { }

            try
            {
                return new HttpClient
                {
                    BaseAddress = new Uri("https://localhost:7190"),
                    Timeout = TimeSpan.FromSeconds(10)
                };
            }
            catch
            {
                return null;
            }
        }

        // INTERNAL: return only missing quiz module IDs
        public async Task<List<int>> GetModulesWithoutQuizAsync(List<int> moduleIds)
        {
            var missing = new List<int>();

            foreach (var moduleId in moduleIds)
            {
                if (await _quizRepo.GetByModuleIdAsync(moduleId) == null)
                    missing.Add(moduleId);
            }

            return missing;
        }

        private static decimal CalculatePercentage(int obtained, int total)
        {
            return total == 0 ? 0 : Math.Round((decimal)obtained / total * 100m, 2);
        }

        private static bool HasAlreadyPassed(QuizSubmission? submission, int totalMarks)
        {
            if (submission?.Score == null) return false;
            return CalculatePercentage(submission.Score.Value, totalMarks) >= PASS_PERCENTAGE;
        }

        private static (int obtained, int correct) EvaluateQuiz(Quiz quiz, SubmitQuizDto dto)
        {
            int obtained = 0;
            int correct = 0;

            foreach (var q in quiz.Questions)
            {
                var submitted = dto.Answers.FirstOrDefault(a => a.QuestionId == q.Id);
                if (submitted == null) continue;

                var correctAns = q.Answers.FirstOrDefault(a => a.IsCorrect);
                if (correctAns != null && submitted.SelectedAnswerId == correctAns.Id)
                {
                    correct++;
                    obtained += q.Marks;
                }
            }

            return (obtained, correct);
        }

        private static QuizResultDto BuildAlreadyPassedResult(
            QuizSubmission submission,
            Quiz quiz,
            int totalMarks,
            int totalQuestions)
        {
            var percentage = CalculatePercentage(submission.Score ?? 0, totalMarks);

            return new QuizResultDto
            {
                SubmissionId = submission.Id,
                TotalQuestions = totalQuestions,
                TotalMarks = totalMarks,
                ObtainedMarks = submission.Score ?? 0,
                Percentage = percentage,
                Passed = true,
                AlreadyPassed = true,
                StatusMessage = "You already passed. No further attempts allowed."
            };
        }

        private static QuizResultDto BuildResult(
            QuizSubmission submission,
            int obtained,
            int correct,
            int totalMarks,
            int totalQuestions,
            decimal percentage)
        {
            return new QuizResultDto
            {
                SubmissionId = submission.Id,
                TotalQuestions = totalQuestions,
                TotalMarks = totalMarks,
                ObtainedMarks = obtained,
                CorrectAnswers = correct,
                Percentage = percentage,
                Passed = percentage >= PASS_PERCENTAGE,
                AlreadyPassed = false,
                StatusMessage = percentage >= PASS_PERCENTAGE
                    ? "🎉 Passed!"
                    : "❌ Try again."
            };
        }

        private static void ValidateQuestion(CreateQuestionDto dto)
        {
            if (dto.Options.Count < 2)
                throw new ArgumentException("At least 2 options required.");

            if (dto.CorrectAnswerIndex < 0 || dto.CorrectAnswerIndex >= dto.Options.Count)
                throw new ArgumentException("CorrectAnswerIndex out of range.");
        }


        // DTO: matches course-service module list
        private class ModuleInfo
        {
            public int Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public string? Content { get; set; }
        }

        // DTO: matches course-service single-module response
        private class ModuleDetail
        {
            public int Id { get; set; }
            public int CourseId { get; set; }
        }
    }
}