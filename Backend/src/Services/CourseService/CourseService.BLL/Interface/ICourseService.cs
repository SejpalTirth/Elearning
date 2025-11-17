using CourseService.BLL.DTOs;
using CourseService.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseService.BLL.Interface
{
    public interface ICourseService
    {
        Task<IEnumerable<Course>> GetAllAsync();
        Task<Course?> GetByIdAsync(int id);
        Task<Course> CreateAsync(CourseDto dto);
        Task<Course> UpdateAsync(int id, CourseDto dto);
        Task<bool> EnrollUserAsync(EnrollRequestDto dto);
    }
}
