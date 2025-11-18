using ProgressService.BLL.Models;

namespace ProgressService.BLL.Interface
{
    public interface IProgressService
    {
        Task<List<ProgressDto>> GetUserProgressAsync(Guid userId);
        Task<SummaryDto> GetSummaryAsync(Guid userId);
    }
}
