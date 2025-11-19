using AssessmentService.BLL.DTOs;

namespace AssessmentService.BLL.Interfaces
{
    public interface IAssessmentService
    {
        Task<IEnumerable<QuizDto>> GetAllQuizzesAsync();
        Task<QuizDto?> GetQuizByIdAsync(int quizId);
        Task<SubmissionResultDto> SubmitQuizAsync(SubmitQuizDto dto);
    }
}
