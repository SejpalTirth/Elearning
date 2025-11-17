using CourseService.BLL.DTOs;
using CourseService.BLL.Interface;
using CourseService.DAL.Models;
using CourseService.DAL.Repo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseService.BLL.Service
{
    public class Courseservice : ICourseService
    {
        private readonly ICourseRepository _repo;
        private readonly IEnrollmentRepository _enrollRepo;

        public Courseservice(ICourseRepository repo, IEnrollmentRepository enrollRepo)
        {
            _repo = repo;
            _enrollRepo = enrollRepo;
        }

        public async Task<IEnumerable<Course>> GetAllAsync()
            => await _repo.GetAllAsync();

        public async Task<Course?> GetByIdAsync(int id)
            => await _repo.GetByIdAsync(id);

        public async Task<Course> CreateAsync(CourseDto dto)
        {
            var course = new Course
            {
                Title = dto.Title,
                Description = dto.Description,
                CategoryId = dto.CategoryId
            };

            await _repo.AddAsync(course);
            await _repo.SaveChangesAsync();
            return course;
        }

        public async Task<Course> UpdateAsync(int id, CourseDto dto)
        {
            var course = await _repo.GetByIdAsync(id);
            if (course == null) return null;

            course.Title = dto.Title;
            course.Description = dto.Description;
            course.CategoryId = dto.CategoryId;

            await _repo.UpdateAsync(course);
            await _repo.SaveChangesAsync();

            return course;
        }

        public async Task<bool> EnrollUserAsync(EnrollRequestDto dto)
        {
            var enrollment = new Enrollment
            {
                CourseId = dto.CourseId,
                UserId = dto.UserId
            };

            await _enrollRepo.AddAsync(enrollment);
            await _enrollRepo.SaveChangesAsync();

            return true;
        }
    }
}
