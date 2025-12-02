using CourseService.BLL.DTOs;
using CourseService.DAL.Models;

namespace CourseService.BLL.Interface
{
    public interface ICourseService
    {
        Task<IEnumerable<CourseResponseDto>> GetAllAsync();
        Task<CourseResponseDto?> GetByIdAsync(int id);
        Task<Course> CreateAsync(CourseDto dto);
        Task<Course?> UpdateAsync(int id, UpdateCourseDto dto);
        Task<bool> EnrollUserAsync(EnrollRequestDto dto);
        Task<IEnumerable<Course>> GetUserEnrolledCoursesAsync(string userId);
        Task<IEnumerable<Course>> GetCoursesByInstructorAsync(Guid instructorId);
        Task<bool> DeleteAsync(int id);
        Task<bool> PublishCourseIfReadyAsync(int courseId);
        Task<Course?> GetUnpublishedCourseAsync(Guid instructorUserId);
        Task<object?> GetUnfinishedCourseAsync(Guid instructorId);
        Task<bool> ContinueUnfinishedCourseAsync(int courseId);


    }
}
