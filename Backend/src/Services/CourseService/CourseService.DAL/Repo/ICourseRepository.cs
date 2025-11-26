using CourseService.DAL.Models;

namespace CourseService.DAL.Repo
{
    public interface ICourseRepository
    {
        Task<IEnumerable<Course>> GetAllAsync();
        Task<Course?> GetByIdAsync(int id);
        Task<Course?> GetByIdWithModulesAsync(int id);
        Task AddAsync(Course course);
        Task UpdateAsync(Course course);
        Task<bool> ExistsAsync(int id);
        Task SaveChangesAsync();
        Task<IEnumerable<Course>> GetByIdsAsync(List<int> ids);
        Task<IEnumerable<Course>> GetByInstructorIdAsync(Guid instructorId);
    }
}
