using AssessmentService.BLL.DTOs;

namespace AssessmentService.BLL.Interfaces;

public interface IAssessmentService
{
    Task<object> CreateQuizAsync(CreateQuizDto dto);
    Task<object> AddQuestionAsync(int quizId, CreateQuestionDto dto);

    Task<QuizForModuleDto?> GetQuizForModuleAsync(int moduleId, Guid userId);

    Task<QuizResultDto> SubmitQuizAsync(SubmitQuizDto dto);

    Task<QuizResultDto?> GetSubmissionResultAsync(Guid submissionId);

    Task<IEnumerable<QuizSummaryDto>> GetAllQuizzesAsync();

    Task<QuizDetailDto?> GetQuizByIdAsync(int id);

    Task<object> GetQuizStatusForCourseAsync(int courseId);

    Task<List<int>> GetModulesWithoutQuizAsync(List<int> moduleIds);
    Task<List<int>> GetModulesWithoutQuizByCourseAsync(int courseId);
}
