using AssessmentService.DAL.Models;

namespace AssessmentService.DAL.Repo
{
    public interface IQuizRepository
    {
        Task<Quiz?> GetByModuleIdAsync(int moduleId);
        Task AddAsync(Quiz quiz);
        Task SaveChangesAsync();
        Task<Quiz?> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<Quiz>> GetAllAsync();
        Task<Quiz?> GetByIdAsync(int id);
        Task<List<int>> GetModuleIdsWithQuizAsync(List<int> moduleIds);

    }

}
