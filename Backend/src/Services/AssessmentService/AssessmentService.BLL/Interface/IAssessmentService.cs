using DTOs._4AssessmentService;

namespace AssessmentService.BLL.Interfaces;
public interface IAssessmentService
{
    Task<CreateQuizResponseDto> CreateQuizAsync(CreateQuizDto dto);
    Task<AddQuestionResponseDto> AddQuestionAsync(AddQuestionDto dto);

    Task<QuizForModuleDto?> GetQuizForModuleAsync(int moduleId, Guid userId);

    Task<QuizResultDto> SubmitQuizAsync(SubmitQuizDto dto);

    Task<QuizResultDto?> GetSubmissionResultAsync(Guid submissionId);

    Task<IEnumerable<QuizSummaryDto>> GetAllQuizzesAsync();

    Task<QuizDetailDto?> GetQuizByIdAsync(int id);

    Task<CourseQuizStatusResponseDto> GetQuizStatusForCourseAsync(int courseId);

    Task<List<int>> GetModulesWithoutQuizByCourseAsync(int courseId);
}
