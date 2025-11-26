using AssessmentService.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace AssessmentService.DAL.Repo
{
    public class QuizRepository : IQuizRepository
    {
        private readonly AssessmentDbContext _db;

        public QuizRepository(AssessmentDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Quiz>> GetAllAsync()
        {
            return await _db.Quizzes
                .Include(q => q.Questions)
                .ThenInclude(a => a.Answers)
                .ToListAsync();
        }

        public async Task<Quiz?> GetByModuleIdAsync(int moduleId)
        {
            return await _db.Quizzes
               .Include(q => q.Questions)
               .ThenInclude(a => a.Answers)
               .FirstOrDefaultAsync(q => q.ModuleId == moduleId);
        }

        public async Task<Quiz?> GetByIdWithDetailsAsync(int quizId)
        {
            return await _db.Quizzes
                .Include(q => q.Questions)
                .ThenInclude(a => a.Answers)
                .FirstOrDefaultAsync(q => q.Id == quizId);
        }

        public async Task AddAsync(Quiz quiz)
        {
            await _db.Quizzes.AddAsync(quiz);
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }

        public async Task<Quiz?> GetByIdAsync(int id)
        {
            return await _db.Quizzes.FirstOrDefaultAsync(q => q.Id == id);
        }
            
    }
}
