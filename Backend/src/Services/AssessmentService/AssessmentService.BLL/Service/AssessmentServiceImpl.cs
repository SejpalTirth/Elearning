using AssessmentService.BLL.DTOs;
using AssessmentService.BLL.Interfaces;
using AssessmentService.DAL.Models;
using AssessmentService.DAL.Repo;
using AutoMapper;
using Microsoft.AspNetCore.Http;
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
        private readonly IHttpContextAccessor _httpContextAccessor;

        private const decimal PASS_PERCENTAGE = 67m;

        public AssessmentServiceImpl(
            IQuizRepository quizRepo,
            ISubmissionRepository submissionRepo,
            IQuestionRepository questionRepo,
            IHttpClientFactory httpFactory,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor)
        {
            _quizRepo = quizRepo;
            _submissionRepo = submissionRepo;
            _questionRepo = questionRepo;
            _httpFactory = httpFactory;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        // ======================= CREATE QUIZ =======================

        public async Task<object> CreateQuizAsync(CreateQuizDto dto)
        {
            var existing = await _quizRepo.GetByModuleIdAsync(dto.ModuleId);
            if (existing != null)
                return new { message = "Quiz already exists.", quizId = existing.Id };

            var quiz = new Quiz
            {
                ModuleId = dto.ModuleId,
                Title = dto.Title,
                TimeLimitMinutes = dto.TimeLimitMinutes,
                TotalMarks = 0
            };

            await _quizRepo.AddAsync(quiz);
            await _quizRepo.SaveChangesAsync();

            // Auto-publish course if ready
            try
            {
                var client = CreateCourseServiceClient();
                if (client != null)
                {
                    var moduleResp = await client.PostAsJsonAsync(
                        "/api/course/module",
                        new { moduleId = dto.ModuleId }
                    );

                    if (moduleResp.IsSuccessStatusCode)
                    {
                        var module = await moduleResp.Content
                            .ReadFromJsonAsync<ModuleDetail>();

                        if (module != null)
                        {
                            await client.PostAsJsonAsync(
                                "/api/course/publish",
                                new { courseId = module.CourseId }
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[AutoPublish] " + ex.Message);
            }

            return new { quiz.Id, quiz.Title, quiz.ModuleId };
        }
        public async Task<object> AddQuestionAsync(AddQuestionDto dto)
        {
            var quiz = await _quizRepo.GetByIdAsync(dto.QuizId);
            if (quiz == null)
                return new { message = "Quiz not found." };

            ValidateQuestion(dto.Question);

            var question = _mapper.Map<Question>(dto.Question);
            question.QuizId = dto.QuizId;

            for (int i = 0; i < dto.Question.Options.Count; i++)
            {
                question.Answers.Add(new Answer
                {
                    AnswerText = dto.Question.Options[i],
                    IsCorrect = i == dto.Question.CorrectAnswerIndex
                });
            }

            await _questionRepo.AddAsync(question);

            quiz.TotalMarks = (quiz.TotalMarks ?? 0) + dto.Question.Marks;
            await _quizRepo.SaveChangesAsync();

            return new
            {
                message = "Question added successfully.",
                questionId = question.Id
            };
        }


        // ======================= QUIZ FETCH =======================

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

            var percentage = CalculatePercentage(submission.Score ?? 0, totalMarks);

            return new QuizResultDto
            {
                SubmissionId = submission.Id,
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

        // ======================= SUBMIT QUIZ =======================

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

        // ======================= UNQUIZZED MODULES =======================

        public async Task<List<int>> GetModulesWithoutQuizByCourseAsync(int courseId)
        {
            var modules = await FetchModulesForCourseAsync(courseId);
            if (modules == null || !modules.Any())
                return new List<int>();

            var moduleIds = modules.Select(m => m.Id).ToList();
            return await GetModulesWithoutQuizAsync(moduleIds);
        }

        // ======================= QUIZ STATUS =======================

        public async Task<object> GetQuizStatusForCourseAsync(int courseId)
        {
            var modules = await FetchModulesForCourseAsync(courseId);

            if (modules == null)
            {
                return new
                {
                    courseId,
                    allQuizzesCreated = false,
                    modules = new List<object>(),
                    nextPendingModuleId = (int?)null,
                    error = "CourseService unavailable"
                };
            }

            if (!modules.Any())
            {
                return new
                {
                    courseId,
                    allQuizzesCreated = false,
                    modules = new List<object>(),
                    nextPendingModuleId = (int?)null
                };
            }

            var missing = await GetModulesWithoutQuizAsync(modules.Select(m => m.Id).ToList());

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
                nextPendingModuleId =
                    moduleStatus.FirstOrDefault(m => !m.quizExists)?.moduleId
            };
        }

        // ======================= HELPERS =======================

        private async Task<List<ModuleInfo>?> FetchModulesForCourseAsync(int courseId)
        {
            try
            {
                var client = CreateCourseServiceClient();
                if (client == null) return null;

                var response = await client.PostAsJsonAsync(
                    "/api/courses/modules",
                    new { courseId }
                );

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<List<ModuleInfo>>(
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
            }
            catch
            {
                return null;
            }
        }

        private HttpClient? CreateCourseServiceClient()
        {
            try
            {
                var client = _httpFactory.CreateClient("CourseService");

                var token = _httpContextAccessor
                    .HttpContext?
                    .Request
                    .Headers["Authorization"]
                    .FirstOrDefault();

                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue(
                            "Bearer",
                            token.Replace("Bearer ", "")
                        );
                }

                return client;
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<int>> GetModulesWithoutQuizAsync(List<int> moduleIds)
        {
            var missing = new List<int>();
            foreach (var id in moduleIds)
            {
                if (await _quizRepo.GetByModuleIdAsync(id) == null)
                    missing.Add(id);
            }
            return missing;
        }

        private static decimal CalculatePercentage(int obtained, int total)
            => total == 0 ? 0 : Math.Round((decimal)obtained / total * 100m, 2);

        private static bool HasAlreadyPassed(QuizSubmission? submission, int total)
            => submission?.Score != null &&
               CalculatePercentage(submission.Score.Value, total) >= PASS_PERCENTAGE;

        private static (int obtained, int correct) EvaluateQuiz(Quiz quiz, SubmitQuizDto dto)
        {
            int obtained = 0, correct = 0;

            foreach (var q in quiz.Questions)
            {
                var submitted = dto.Answers.FirstOrDefault(a => a.QuestionId == q.Id);
                if (submitted == null) continue;

                var correctAns = q.Answers.FirstOrDefault(a => a.IsCorrect);
                if (correctAns != null && submitted.SelectedAnswerId == correctAns.Id)
                {
                    obtained += q.Marks;
                    correct++;
                }
            }

            return (obtained, correct);
        }

        private static QuizResultDto BuildAlreadyPassedResult(
            QuizSubmission submission, Quiz quiz, int totalMarks, int totalQuestions)
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
                StatusMessage = "Already passed."
            };
        }

        private static QuizResultDto BuildResult(
            QuizSubmission submission, int obtained, int correct,
            int totalMarks, int totalQuestions, decimal percentage)
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
                StatusMessage = percentage >= PASS_PERCENTAGE ? "🎉 Passed!" : "❌ Try again."
            };
        }

        private static void ValidateQuestion(CreateQuestionDto dto)
        {
            if (dto.Options.Count < 2)
                throw new ArgumentException("At least 2 options required.");
            if (dto.CorrectAnswerIndex < 0 || dto.CorrectAnswerIndex >= dto.Options.Count)
                throw new ArgumentException("CorrectAnswerIndex out of range.");
        }

        private class ModuleInfo
        {
            public int Id { get; set; }
            public string Title { get; set; } = string.Empty;
        }

        private class ModuleDetail
        {
            public int Id { get; set; }
            public int CourseId { get; set; }
        }
    }
}
