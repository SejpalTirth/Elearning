using AssessmentService.DAL.Models;

namespace AssessmentService.DAL.Repo
{
    public interface IQuestionRepository
    {
        Task AddAsync(Question question);
        Task SaveChangesAsync();
    }
}
