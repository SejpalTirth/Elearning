using AssessmentService.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace AssessmentService.DAL.Repo
{
    public class QuizRepository : IQuizRepository
    {
        private readonly AssessmentDbContext _context;

        public QuizRepository(AssessmentDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Quizzes>> GetAllQuizzesAsync()
        {
            return await _context.Quizzes
                .Include(q => q.Questions)
                .ThenInclude(q => q.Answers)
                .ToListAsync();
        }

        public async Task<Quizzes?> GetQuizByIdAsync(int quizId)
        {
            return await _context.Quizzes
                .Include(q => q.Questions)
                .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == quizId);

        }
    }
}
