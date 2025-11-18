using ProgresService.DAL.Models;

namespace ProgresService.DAL.Repo
{
    public interface IProgressRepository
    {
        Task<List<ProgressTracking>> GetUserProgressAsync(Guid userId);
        Task<int> GetCompletedCourseCountAsync(Guid userId);
    }
}
