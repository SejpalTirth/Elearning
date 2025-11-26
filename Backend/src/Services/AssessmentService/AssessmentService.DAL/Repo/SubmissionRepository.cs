using AssessmentService.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace AssessmentService.DAL.Repo
{
    public class SubmissionRepository : ISubmissionRepository
    {
        private readonly AssessmentDbContext _db;

        public SubmissionRepository(AssessmentDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(QuizSubmission submission)
        {
            await _db.QuizSubmissions.AddAsync(submission);
        }

        public async Task<QuizSubmission?> GetBestSubmissionAsync(int quizId, Guid userId)
        {
            return await _db.QuizSubmissions
                .Where(s => s.QuizId == quizId && s.UserId == userId)
                .OrderByDescending(s => s.Score)
                .FirstOrDefaultAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }

        public async Task<QuizSubmission?> GetByIdAsync(Guid id)
        {
            return await _db.QuizSubmissions.FirstOrDefaultAsync(x => x.Id == id);
        }

    }
}
