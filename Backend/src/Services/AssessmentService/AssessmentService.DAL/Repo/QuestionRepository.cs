using AssessmentService.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace AssessmentService.DAL.Repo
{
    public class QuestionRepository : IQuestionRepository
    {
        private readonly AssessmentDbContext _context;

        public QuestionRepository(AssessmentDbContext context)
        {
            _context = context;
        }

        public async Task<Questions?> GetQuestionByIdAsync(int questionId)
        {
            return await _context.Questions
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == questionId);
        }
    }
}
