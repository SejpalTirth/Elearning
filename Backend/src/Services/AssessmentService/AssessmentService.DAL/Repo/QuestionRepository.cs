using AssessmentService.DAL.Models;

namespace AssessmentService.DAL.Repo
{
    public class QuestionRepository : IQuestionRepository
    {
        private readonly AssessmentDbContext _db;

        public QuestionRepository(AssessmentDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(Question question)
        {
            await _db.Questions.AddAsync(question);
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
