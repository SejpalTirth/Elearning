using AssessmentService.DAL.Models;

namespace AssessmentService.DAL.Repo
{
    public interface ISubmissionRepository
    {
        Task<QuizSubmissions> AddSubmissionAsync(QuizSubmissions submission);
        Task SaveChangesAsync();
    }
}
