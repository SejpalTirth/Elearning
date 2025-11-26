using CourseService.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseService.DAL.Repo
{
    public class ModuleRepository : IModuleRepository
    {
        private readonly CourseContext _context;

        public ModuleRepository(CourseContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Module>> GetByCourseIdAsync(int courseId)
        {
            return await _context.Modules
                .Where(m => m.CourseId == courseId)
                .ToListAsync();
        }

        public async Task<Module?> GetByIdAsync(int id)
        {
            return await _context.Modules
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task AddAsync(Module module)
        {
            await _context.Modules.AddAsync(module);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
