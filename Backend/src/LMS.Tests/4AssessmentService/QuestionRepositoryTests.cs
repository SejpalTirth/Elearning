using AutoFixture;
using AssessmentService.DAL.Models;
using AssessmentService.DAL.Repo;

namespace LMS.Tests.AssessmentService
{
    public class QuestionRepositoryTests : BaseTest
    {
        private readonly QuestionRepository _repo;
        private readonly IFixture _fixture;

        public QuestionRepositoryTests()
        {
            _repo = new QuestionRepository(AssessmentContext);
            _fixture = new Fixture();
        }
        [Fact]
        public async Task AddAsync_ShouldAddQuestionToDatabase()
        {
            var question = _fixture.Build<Question>()
                .Without(q => q.Quiz)
                .Without(q => q.Answers)
                .With(q => q.QuestionText, "What is 2+2?")
                .With(q => q.QuestionType, "MCQ")
                .With(q => q.Marks, 4)
                .Create();

            await _repo.AddAsync(question);
            await _repo.SaveChangesAsync();

            var saved = AssessmentContext.Questions.First();

            Assert.Equal("What is 2+2?", saved.QuestionText);
            Assert.Equal(4, saved.Marks);
        }

    }
}
