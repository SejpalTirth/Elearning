using AssessmentService.BLL.DTOs;
using AssessmentService.BLL.Interfaces;
using AssessmentService.DAL.Models;
using AssessmentService.DAL.Repo;
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

        private const decimal PASS_PERCENTAGE = 67m;

        public AssessmentServiceImpl(
            IQuizRepository quizRepo,
            ISubmissionRepository submissionRepo,
            IQuestionRepository questionRepo,
            IHttpClientFactory httpFactory)
        {
            _quizRepo = quizRepo;
            _submissionRepo = submissionRepo;
            _questionRepo = questionRepo;
            _httpFactory = httpFactory;
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
        public async Task<IEnumerable<object>> GetAllQuizzesAsync()
        {
            var data = await _quizRepo.GetAllAsync();
            return data.Select(q => new { q.Id, q.Title });
        }

        public async Task<object?> GetQuizByIdAsync(int id)
        {
            var quiz = await _quizRepo.GetByIdWithDetailsAsync(id);
            if (quiz == null) return null;

            return new
            {
                quiz.Id,
                quiz.Title,
                quiz.TotalMarks,
                Questions = quiz.Questions.Select(q => new
                {
                    q.Id,
                    q.QuestionText,
                    q.Marks
                })
            };
        }

        public async Task<object?> GetSubmissionResultAsync(Guid submissionId)
        {
            var submission = await _submissionRepo.GetByIdAsync(submissionId);
            if (submission == null) return null;

            var quiz = await _quizRepo.GetByIdAsync(submission.QuizId);
            if (quiz == null) return null;

            int totalMarks = quiz.TotalMarks ?? quiz.Questions.Sum(q => q.Marks);
            var percentage = Math.Round((decimal)(submission.Score ?? 0) / totalMarks * 100m, 2);

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
            if (quiz == null)
                return new { message = "Quiz not found." };

            if (dto.Options == null || dto.Options.Count < 2)
                return new { message = "A question must have minimum 2 options." };

            if (dto.CorrectAnswerIndex < 0 || dto.CorrectAnswerIndex >= dto.Options.Count)
                return new { message = "CorrectAnswerIndex is out of range." };

            var question = new Question
            {
                QuizId = quizId,
                QuestionText = dto.Text,
                QuestionType = "MCQ",
                Marks = dto.Marks
            };

            foreach (var (option, index) in dto.Options.Select((o, i) => (o, i)))
            {
                question.Answers.Add(new Answer
                {
                    AnswerText = option,
                    IsCorrect = index == dto.CorrectAnswerIndex
                });
            }

            await _questionRepo.AddAsync(question);
            await _questionRepo.SaveChangesAsync();

            quiz.TotalMarks = (quiz.TotalMarks ?? 0) + dto.Marks;
            await _quizRepo.SaveChangesAsync();

            return new { message = "Question added successfully.", questionId = question.Id };
        }

        // GET QUIZ FOR MODULE
        public async Task<object?> GetQuizForModuleAsync(int moduleId, Guid userId)
        {
            var quiz = await _quizRepo.GetByModuleIdAsync(moduleId);
            if (quiz == null) return null;

            var totalMarks = quiz.Questions.Sum(q => q.Marks);
            var best = await _submissionRepo.GetBestSubmissionAsync(quiz.Id, userId);

            bool alreadyPassed = false;
            if (best != null && best.Score.HasValue)
            {
                var percent = Math.Round((decimal)best.Score.Value / totalMarks * 100m, 2);
                alreadyPassed = percent >= PASS_PERCENTAGE;
            }

            return new
            {
                QuizId = quiz.Id,
                ModuleId = moduleId,
                Title = quiz.Title,
                TimeLimitMinutes = quiz.TimeLimitMinutes,
                TotalMarks = totalMarks,
                AlreadyPassed = alreadyPassed,
                Questions = quiz.Questions.Select(q => new
                {
                    QuestionId = q.Id,
                    Text = q.QuestionText,
                    Marks = q.Marks,
                    Answers = q.Answers.Select(a => new { a.Id, a.AnswerText })
                }).ToList()
            };
        }

        // SUBMIT QUIZ
        public async Task<object> SubmitQuizAsync(SubmitQuizDto dto)
        {
            var quiz = await _quizRepo.GetByIdWithDetailsAsync(dto.QuizId);
            if (quiz == null)
                return new { message = "Quiz not found." };

            int totalMarks = quiz.Questions.Sum(q => q.Marks);
            int totalQuestions = quiz.Questions.Count;

            var previous = await _submissionRepo.GetBestSubmissionAsync(dto.QuizId, dto.UserId);

            if (previous != null && previous.Score.HasValue)
            {
                var prevPercentage = Math.Round((decimal)previous.Score.Value / totalMarks * 100m, 2);
                if (prevPercentage >= PASS_PERCENTAGE)
                {
                    return new QuizResultDto
                    {
                        SubmissionId = previous.Id,
                        TotalQuestions = totalQuestions,
                        TotalMarks = totalMarks,
                        ObtainedMarks = previous.Score ?? 0,
                        Percentage = prevPercentage,
                        Passed = true,
                        AlreadyPassed = true,
                        StatusMessage = "You already passed. No further attempts allowed."
                    };
                }
            }

            int obtainedMarks = 0;
            int correctAnswers = 0;

            foreach (var q in quiz.Questions)
            {
                var submitted = dto.Answers.FirstOrDefault(x => x.QuestionId == q.Id);
                if (submitted == null) continue;

                var correctAns = q.Answers.FirstOrDefault(x => x.IsCorrect);
                if (correctAns != null && submitted.SelectedAnswerId == correctAns.Id)
                {
                    correctAnswers++;
                    obtainedMarks += q.Marks;
                }
            }

            decimal percentage = Math.Round((decimal)obtainedMarks / totalMarks * 100m, 2);
            bool passed = percentage >= PASS_PERCENTAGE;

            var submission = new QuizSubmission
            {
                QuizId = quiz.Id,
                UserId = dto.UserId,
                Score = obtainedMarks,
                SubmittedData = JsonSerializer.Serialize(dto.Answers)
            };

            await _submissionRepo.AddAsync(submission);
            await _submissionRepo.SaveChangesAsync();

            return new QuizResultDto
            {
                SubmissionId = submission.Id,
                TotalQuestions = totalQuestions,
                TotalMarks = totalMarks,
                ObtainedMarks = obtainedMarks,
                CorrectAnswers = correctAnswers,
                Percentage = percentage,
                Passed = passed,
                AlreadyPassed = false,
                StatusMessage = passed
                    ? $"🎉 Passed! You scored {obtainedMarks}/{totalMarks}."
                    : $"❌ You scored {obtainedMarks}/{totalMarks}. Minimum passing: {PASS_PERCENTAGE}%"
            };
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
