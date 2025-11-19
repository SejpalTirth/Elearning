using Microsoft.EntityFrameworkCore;
using ProgresService.DAL.Data;
using ProgresService.DAL.Models;
using ProgresService.DAL.Repo;

namespace ProgressService.DAL.Repo
{
    public class ProgressRepository : IProgressRepository
    {
        private readonly ProgressDbContext _context;

        public ProgressRepository(ProgressDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProgressTracking>> GetUserProgressAsync(Guid userId)
        {
            return await _context.ProgressTrackings
                .Where(p => p.UserId == userId)
                .ToListAsync();
        }

        public async Task<int> GetCompletedCourseCountAsync(Guid userId)
        {
            return await _context.CourseCompletions
                .Where(c => c.UserId == userId && c.CompletedAt != null)
                .CountAsync();
        }
    }
}
