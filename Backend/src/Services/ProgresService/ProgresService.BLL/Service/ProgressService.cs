using ProgresService.DAL.Repo;
using ProgressService.BLL.Interface;
using ProgressService.BLL.Models;

namespace ProgressService.BLL.Service
{
    public class ProgressServiceImpl : IProgressService
    {
        private readonly IProgressRepository _repo;

        public ProgressServiceImpl(IProgressRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<ProgressDto>> GetUserProgressAsync(Guid userId)
        {
            var progress = await _repo.GetUserProgressAsync(userId);

            return progress.Select(p => new ProgressDto
            {
                CourseId = p.CourseId,
                ModuleId = p.ModuleId,
                ProgressPercent = p.ProgressPercent,
                LastUpdated = p.LastUpdated
            }).ToList();
        }

        public async Task<SummaryDto> GetSummaryAsync(Guid userId)
        {
            var progress = await _repo.GetUserProgressAsync(userId);
            var completedCourses = await _repo.GetCompletedCourseCountAsync(userId);

            int totalCourses = progress.Select(p => p.CourseId).Distinct().Count();

            decimal avg = progress.Any()
                ? progress.Average(p => p.ProgressPercent)
                : 0;

            return new SummaryDto
            {
                TotalCourses = totalCourses,
                CompletedCourses = completedCourses,
                AverageProgress = Math.Round(avg, 2)
            };
        }
    }
}
