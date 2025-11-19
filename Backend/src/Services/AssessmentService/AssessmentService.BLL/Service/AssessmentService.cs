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

        public AssessmentServiceImpl(
            IQuizRepository quizRepo,
            ISubmissionRepository submissionRepo)
        {
            _quizRepo = quizRepo;
            _submissionRepo = submissionRepo;
        }

        public async Task<IEnumerable<QuizDto>> GetAllQuizzesAsync()
        {
            var quizzes = await _quizRepo.GetAllQuizzesAsync();

            return quizzes.Select(q => new QuizDto
            {
                Id = q.Id,
                Title = q.Title,
                Questions = q.Questions.Select(ques => new QuestionDto
                {
                    Id = ques.Id,
                    QuestionText = ques.QuestionText,
                    Answers = ques.Answers.Select(a => new AnswerDto
                    {
                        Id = a.Id,
                        AnswerText = a.AnswerText
                    }).ToList()
                }).ToList()
            });
        }

        public async Task<QuizDto?> GetQuizByIdAsync(int quizId)
        {
            var quiz = await _quizRepo.GetQuizByIdAsync(quizId);

            if (quiz == null)
                return null;

            return new QuizDto
            {
                Id = quiz.Id,
                Title = quiz.Title,
                Questions = quiz.Questions.Select(q => new QuestionDto
                {
                    Id = q.Id,
                    QuestionText = q.QuestionText,
                    Answers = q.Answers.Select(a => new AnswerDto
                    {
                        Id = a.Id,
                        AnswerText = a.AnswerText
                    }).ToList()
                }).ToList()
            };
        }

        public async Task<SubmissionResultDto> SubmitQuizAsync(SubmitQuizDto dto)
        {
            var quiz = await _quizRepo.GetQuizByIdAsync(dto.QuizId);

            if (quiz == null)
                throw new Exception("Quiz not found");

            int correct = 0;
            int total = quiz.Questions.Count;

            foreach (var studentAnswer in dto.Answers)
            {
                var question = quiz.Questions
                    .FirstOrDefault(q => q.Id == studentAnswer.QuestionId);

                if (question == null)
                    continue;

                var selectedAnswer = question.Answers
                    .FirstOrDefault(a => a.Id == studentAnswer.SelectedAnswerId);

                if (selectedAnswer != null && selectedAnswer.IsCorrect == true)
                {
                    correct++;
                }
            }

            double percentage = total == 0 ? 0 : (double)correct / total * 100;

            // Save submission result
            var submission = new QuizSubmissions
            {
                QuizId = dto.QuizId,
                UserId = dto.UserId,
                Score = correct,
                SubmittedAt = DateTime.Now,
                SubmittedData = System.Text.Json.JsonSerializer.Serialize(dto.Answers)
            };

            await _submissionRepo.AddSubmissionAsync(submission);

            return new SubmissionResultDto
            {
                TotalQuestions = total,
                CorrectAnswers = correct,
                Percentage = percentage
            };
        }

    }
}
