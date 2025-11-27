using AssessmentService.BLL.DTOs;
using AssessmentService.BLL.Interfaces;
using AssessmentService.DAL.Models;
using AssessmentService.DAL.Repo;

namespace AssessmentService.BLL.Services
{
    public class AssessmentServiceImpl : IAssessmentService
    {
        private readonly IQuizRepository _quizRepo;
        private readonly ISubmissionRepository _submissionRepo;
        private readonly IQuestionRepository _questionRepo;

        private const decimal PASS_PERCENTAGE = 67m;

        public AssessmentServiceImpl(
            IQuizRepository quizRepo,
            ISubmissionRepository submissionRepo,
            IQuestionRepository questionRepo)
        {
            _quizRepo = quizRepo;
            _submissionRepo = submissionRepo;
            _questionRepo = questionRepo;
        }

        // ---------------------------------------------------------
        // CREATE QUIZ (Instructor Side - minimal)
        // ---------------------------------------------------------
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

            return new { quiz.Id, quiz.Title, quiz.ModuleId };
        }

        public async Task<object> AddQuestionAsync(int quizId, CreateQuestionDto dto)
        {
            // 1. Validate quiz exists
            var quiz = await _quizRepo.GetByIdAsync(quizId);
            if (quiz == null)
                return new { message = "Quiz not found." };

            // 2. Validate at least 2 options
            if (dto.Options == null || dto.Options.Count < 2)
                return new { message = "A question must have at least 2 options." };

            // 3. Validate correct answer index
            if (dto.CorrectAnswerIndex < 0 || dto.CorrectAnswerIndex >= dto.Options.Count)
                return new { message = "CorrectAnswerIndex is out of range." };

            // 4. Create Question object
            var question = new Question
            {
                QuizId = quizId,
                QuestionText = dto.Text,
                QuestionType = "MCQ",
                Marks = dto.Marks
            };

            // Add answers to question
            foreach (var (option, index) in dto.Options.Select((o, i) => (o, i)))
            {
                question.Answers.Add(new Answer
                {
                    AnswerText = option,
                    IsCorrect = index == dto.CorrectAnswerIndex
                });
            }

            // 5. Save question
            await _questionRepo.AddAsync(question);
            await _questionRepo.SaveChangesAsync();

            // 6. Update quiz total marks
            quiz.TotalMarks = (quiz.TotalMarks ?? 0) + dto.Marks;
            await _quizRepo.SaveChangesAsync();

            return new
            {
                message = "Question added successfully.",
                questionId = question.Id
            };
        }


        // ---------------------------------------------------------
        // GET QUIZ FOR MODULE (STUDENT)
        // ---------------------------------------------------------
        public async Task<object?> GetQuizForModuleAsync(int moduleId, Guid userId)
        {
            var quiz = await _quizRepo.GetByModuleIdAsync(moduleId);
            if (quiz == null) return null;

            var totalMarks = quiz.Questions.Sum(q => q.Marks);
            var previousBest = await _submissionRepo.GetBestSubmissionAsync(quiz.Id, userId);

            bool alreadyPassed = false;

            if (previousBest != null && previousBest.Score.HasValue)
            {
                var percent = Math.Round((decimal)previousBest.Score.Value / totalMarks * 100m, 2);
                if (percent >= PASS_PERCENTAGE)
                    alreadyPassed = true;
            }

            return new
            {
                QuizId = quiz.Id,
                ModuleId = moduleId,
                Title = quiz.Title,
                TimeLimitMinutes = quiz.TimeLimitMinutes,
                TotalMarks = totalMarks,
                AlreadyPassed = alreadyPassed,
                Questions = quiz.Questions.Select(q => new {
                    QuestionId = q.Id,
                    Text = q.QuestionText,
                    Marks = q.Marks,
                    Answers = q.Answers.Select(a => new { a.Id, a.AnswerText }).ToList()
                }).ToList()
            };
        }


        // ---------------------------------------------------------
        // SUBMIT QUIZ
        // ---------------------------------------------------------
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
                        SubmissionId = previous.Id,   // << ---- REQUIRED!!!
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
                SubmittedData = System.Text.Json.JsonSerializer.Serialize(dto.Answers)
            };

            await _submissionRepo.AddAsync(submission);
            await _submissionRepo.SaveChangesAsync();

            return new QuizResultDto
            {
                SubmissionId = submission.Id,   // << ---- REQUIRED!!!
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

        // Basic listing if ever needed
        public async Task<IEnumerable<object>> GetAllQuizzesAsync() =>
            (await _quizRepo.GetAllAsync()).Select(q => new { q.Id, q.Title });

        public Task<object?> GetQuizByIdAsync(int id) => Task.FromResult<object?>(null);

        public async Task<object?> GetSubmissionResultAsync(Guid submissionId)
        {
            var submission = await _submissionRepo.GetByIdAsync(submissionId);

            if (submission == null)
                return null;

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

    }
}
