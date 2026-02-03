using DTOs._5ProgresService;
using ProgresService.BLL.Models;

namespace ProgresService.BLL.Interface
{
    public interface IProgressService
    {
        Task<List<ProgresDto>> GetUserProgressAsync(Guid userId);
        Task MarkModuleCompletedAsync(Guid userId, int courseId, int moduleId);
    }
}
