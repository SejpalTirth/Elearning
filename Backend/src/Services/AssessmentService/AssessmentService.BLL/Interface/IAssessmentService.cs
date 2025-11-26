using AssessmentService.BLL.DTOs;

namespace AssessmentService.BLL.Interfaces
{
    public interface IAssessmentService
    {
        Task<object> CreateQuizAsync(CreateQuizDto dto);
        Task<object> AddQuestionAsync(int quizId, CreateQuestionDto dto);
        Task<object?> GetQuizForModuleAsync(int moduleId, Guid userId);
        Task<object> SubmitQuizAsync(SubmitQuizDto dto);
        Task<object?> GetSubmissionResultAsync(Guid submissionId);
        Task<IEnumerable<object>> GetAllQuizzesAsync();
        Task<object?> GetQuizByIdAsync(int id);
    }
}
