using AssessmentService.DAL.Models;

namespace AssessmentService.DAL.Repo
{
    public interface IQuestionRepository
    {
        Task<Questions?> GetQuestionByIdAsync(int questionId);
    }
}
