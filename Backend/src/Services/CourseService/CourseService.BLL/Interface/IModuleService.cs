using DTOs._2CourseService;

namespace CourseService.BLL.Interface
{
    public interface IModuleService
    {
        Task<IEnumerable<ModuleSummaryDto>> GetModulesByCourseAsync(int courseId);
        Task<ModuleContentResponseDto?> GetModuleContentAsync(int moduleId);
        Task<ModuleAndCourseIdDTO?> GetModuleAndCourseIdAsync(int moduleId);

    }
}
