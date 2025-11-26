using CourseService.DAL.Models;

namespace CourseService.DAL.Repo
{
    public interface IEnrollmentRepository
    {
        Task<IEnumerable<Enrollment>> GetAllAsync();
        Task<Enrollment?> GetByIdAsync(int id);
        Task<IEnumerable<Enrollment>> GetByUserIdAsync(string userId);
        Task<IEnumerable<Enrollment>> GetByCourseIdAsync(int courseId);
        Task AddAsync(Enrollment enrollment);
        Task RemoveAsync(int id);
        Task<bool> SaveChangesAsync();
        Task<bool> IsUserEnrolledAsync(string userId, int courseId);

    }
}
