using AssessmentService.DAL.Models;

namespace AssessmentService.DAL.Repo
{
    public class SubmissionRepository : ISubmissionRepository
    {
        private readonly AssessmentDbContext _context;

        public SubmissionRepository(AssessmentDbContext context)
        {
            _context = context;
        }

        public async Task<QuizSubmissions> AddSubmissionAsync(QuizSubmissions submission)
        {
            _context.QuizSubmissions.Add(submission);
            await _context.SaveChangesAsync();
            return submission;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
