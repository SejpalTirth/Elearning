using ProgresService.DAL.Models;

namespace ProgresService.DAL.Repo
{
    public interface IProgressRepository
    {
        Task<List<ProgressTracking>> GetUserProgressAsync(Guid userId);
        Task DeleteByModuleIdsAsync(List<int> moduleIds);
        Task MarkModuleCompleteAsync(Guid userId, int courseId, int moduleId);
        Task<bool> IsCourseFullyCompletedAsync(
            Guid userId,
            int courseId,
            int totalModuleCount
        );
        Task<int> GetCompletedModuleCountAsync(Guid userId, int courseId);
        Task SaveChangesAsync();
    }
}
