using CourseService.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseService.DAL.Repo
{
    public class CourseRepository : ICourseRepository
    {
        private readonly CourseContext _db;

        public CourseRepository(CourseContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Course>> GetAllAsync()
        {
            return await _db.Courses
                .Include(c => c.Category)
                .Include(c => c.Modules)
                .ToListAsync();
        }

        public async Task<Course?> GetByIdAsync(int id)
        {
            return await _db.Courses
                .Include(c => c.Category)
                .Include(c => c.Modules)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddAsync(Course course)
        {
            await _db.Courses.AddAsync(course);
        }

        public async Task UpdateAsync(Course course)
        {
            _db.Courses.Update(course);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _db.Courses.AnyAsync(c => c.Id == id);
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
