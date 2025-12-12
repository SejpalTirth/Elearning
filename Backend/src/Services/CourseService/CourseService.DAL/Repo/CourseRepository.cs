using CourseService.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseService.DAL.Repo
{
    public class CourseRepository : ICourseRepository
    {
        private readonly CourseContext _db;

        public CourseRepository(CourseContext db)
        {
            _db = db;
        }

        // -------------------- GET ALL --------------------
        public async Task<IEnumerable<Course>> GetAllAsync()
        {
            return await _db.Courses
                .Include(c => c.Category)
                .Include(c => c.Modules)
                .ToListAsync();
        }
        // -------------------- GET BY ID --------------------
        public async Task<Course?> GetByIdAsync(int id)
        {
            return await _db.Courses
                .Where(c => !c.IsDeleted)
                .Include(c => c.Category)
                .Include(c => c.Modules)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        // -------------------- GET FULL COURSE FOR UPDATE --------------------
        public async Task<Course?> GetByIdWithModulesAsync(int id)
        {
            return await _db.Courses
                .Include(c => c.Modules)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        // -------------------- ADD --------------------
        public async Task AddAsync(Course course)
        {
            await _db.Courses.AddAsync(course);
        }

        // -------------------- UPDATE (Used for Edit + Soft Delete) --------------------
        public async Task UpdateAsync(Course course)
        {
            _db.Courses.Update(course);
        }

        // -------------------- EXISTS CHECK --------------------
        public async Task<bool> ExistsAsync(int id)
        {
            return await _db.Courses.AnyAsync(c => c.Id == id && !c.IsDeleted);
        }

        // -------------------- SAVE DB CHANGES --------------------
        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }

        // -------------------- GET BY MANY IDS --------------------
        public async Task<IEnumerable<Course>> GetByIdsAsync(List<int> ids)
        {
            return await _db.Courses
                .Where(c => ids.Contains(c.Id) && !c.IsDeleted)
                .Include(c => c.Category)
                .Include(c => c.Modules)
                .ToListAsync();
        }

        // -------------------- GET COURSES BY INSTRUCTOR --------------------
        public async Task<IEnumerable<Course>> GetByInstructorIdAsync(Guid instructorId)
        {
            return await _db.Courses
                .Where(c => c.InstructorUserId == instructorId)
                .Include(c => c.Category)
                .Include(c => c.Modules)
                .ToListAsync();
        }
        public async Task<Course?> GetFirstUnpublishedCourse(Guid instructorUserId)
        {
            return await _db.Courses
                .Where(c => c.InstructorUserId == instructorUserId && c.IsDraft == true && c.IsDeleted == false)
                .OrderBy(c => c.Id)
                .FirstOrDefaultAsync();
        }

        public async Task<Course?> GetLatestUnfinishedCourseAsync(Guid instructorId)
        {
            return await _db.Courses
                .Where(c => c.InstructorUserId == instructorId && c.IsDraft == true && c.IsDeleted == false)
                .OrderByDescending(c => c.Id)
                .FirstOrDefaultAsync();
        }

        public async Task<Course?> GetByIdAllowDeletedAsync(int id)
        {
            return await _db.Courses
                .Include(c => c.Modules)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public void RemoveModule(Module module)
        {
            _db.Modules.Remove(module);
        }
        public async Task RestoreAsync(Course course)
        {
            _db.Courses.Update(course);
            await _db.SaveChangesAsync();
        }
    }
}
