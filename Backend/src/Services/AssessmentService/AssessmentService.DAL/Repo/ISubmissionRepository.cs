using AssessmentService.DAL.Models;

namespace AssessmentService.DAL.Repo
{
    public interface ISubmissionRepository
    {
        Task AddAsync(QuizSubmission submission);
        Task SaveChangesAsync();
        Task<QuizSubmission?> GetBestSubmissionAsync(int quizId, Guid userId);

        // NEW
        Task<QuizSubmission?> GetByIdAsync(Guid id);
    }

}
