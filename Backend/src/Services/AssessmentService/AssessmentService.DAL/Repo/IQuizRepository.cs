using AssessmentService.DAL.Models;

namespace AssessmentService.DAL.Repo
{
    public interface IQuizRepository
    {
        Task<IEnumerable<Quizzes>> GetAllQuizzesAsync();
        Task<Quizzes?> GetQuizByIdAsync(int quizId);
    }
}
