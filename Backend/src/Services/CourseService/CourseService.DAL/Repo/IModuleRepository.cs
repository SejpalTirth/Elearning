using CourseService.DAL.Models;

public interface IModuleRepository
{
    Task<IEnumerable<Module>> GetByCourseIdAsync(int courseId);
    Task<Module?> GetByIdAsync(int id);
    Task AddAsync(Module module);
    Task SaveChangesAsync();

}
