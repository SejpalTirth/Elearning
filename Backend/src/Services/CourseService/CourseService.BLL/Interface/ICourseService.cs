using CourseService.BLL.DTOs;
using CourseService.DAL.Models;

namespace CourseService.BLL.Interface
{
    public interface ICourseService
    {
        // Main course operations
        Task<IEnumerable<CourseResponseDto>> GetAllAsync();
        Task<CourseResponseDto?> GetByIdAsync(int id);
        Task<Course> CreateAsync(CourseDto dto);
        Task<Course?> UpdateAsync(int id, UpdateCourseDto dto);
        Task<bool> DeleteAsync(int id);

        // Enrollment
        Task<bool> EnrollUserAsync(
            Guid userId,
            int courseId,
            string userEmail,
            string authorizationHeader
        );
        Task<IEnumerable<Course>> GetUserEnrolledCoursesAsync(string userId);

        // Instructor-specific
        Task<IEnumerable<Course>> GetCoursesByInstructorAsync(Guid instructorId);

        // Publish logic
        Task<bool> PublishCourseIfReadyAsync(int courseId, string authorizationHeader);

        // Pending task system (final)
        Task<IEnumerable<Course>> GetAllUnfinishedCoursesAsync(Guid instructorId);
        Task<bool> ContinueUnfinishedCourseAsync(int courseId);
        Task<bool> RestoreAsync(int id);
    }
}
