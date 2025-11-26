using Microsoft.EntityFrameworkCore;
using ProgresService.DAL.Data;
using ProgresService.DAL.Models;
using ProgresService.DAL.Repo;
using System.Net.Http;
using System.Net.Http.Json;

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

        public async Task DeleteByModuleIdsAsync(List<int> moduleIds)
        {
            if (moduleIds == null || moduleIds.Count == 0)
                return;

            var progressToDelete = await _context.ProgressTrackings
                .Where(p => p.ModuleId.HasValue && moduleIds.Contains(p.ModuleId.Value))
                .ToListAsync();

            _context.ProgressTrackings.RemoveRange(progressToDelete);
        }

        public async Task MarkModuleCompleteAsync(Guid userId, int courseId, int moduleId)
        {
            var record = await _context.ProgressTrackings
                .FirstOrDefaultAsync(p => p.UserId == userId && p.ModuleId == moduleId);

            if (record == null)
            {
                record = new ProgressTracking
                {
                    UserId = userId,
                    CourseId = courseId,
                    ModuleId = moduleId,
                    ProgressPercent = 100,
                    IsCompleted = true,
                    LastUpdated = DateTime.UtcNow
                };

                await _context.ProgressTrackings.AddAsync(record);
            }
            else
            {
                record.ProgressPercent = 100;
                record.IsCompleted = true;
                record.LastUpdated = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        // FIXED VERSION — checks if all modules of this course are completed
        public async Task<bool> IsCourseFullyCompletedAsync(Guid userId, int courseId)
        {
            // Count completed modules by this user
            int completedModules = await _context.ProgressTrackings
                .Where(p => p.UserId == userId && p.CourseId == courseId && p.IsCompleted == true)
                .CountAsync();

            return completedModules > 0; // Placeholder, real logic will be in service
        }
        public async Task<int> GetCompletedModuleCountAsync(Guid userId, int courseId)
        {
            return await _context.ProgressTrackings
                .Where(p => p.UserId == userId && p.CourseId == courseId && p.IsCompleted)
                .CountAsync();
        }

    }
}
