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

            // Prevent recursive navigation loops (Question → Answers → Question…)
            _fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));

            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        // -------------------------------------------------------------
        // ADD QUESTION
        // -------------------------------------------------------------
        [Fact]
        public async Task AddAsync_ShouldAddQuestionToDatabase()
        {
            // Arrange
            var question = _fixture.Build<Question>()
                .Without(q => q.Quiz)
                .Without(q => q.Answers)
                .With(q => q.QuestionText, "What is 2+2?")
                .With(q => q.QuestionType, "MCQ")
                .With(q => q.Marks, 4)
                .Create();

            // Act
            await _repo.AddAsync(question);
            await _repo.SaveChangesAsync();

            // Assert
            var saved = AssessmentContext.Questions.First();
            Assert.Equal("What is 2+2?", saved.QuestionText);
            Assert.Equal(4, saved.Marks);
        }

        // -------------------------------------------------------------
        // SAVE CHANGES
        // -------------------------------------------------------------
        [Fact]
        public async Task SaveChangesAsync_ShouldPersistChanges()
        {
            // Arrange
            var question = _fixture.Build<Question>()
                .Without(q => q.Quiz)
                .Without(q => q.Answers)
                .With(q => q.QuestionText, "Persist Test")
                .With(q => q.Marks, 5)
                .Create();

            await _repo.AddAsync(question);

            // Act
            await _repo.SaveChangesAsync();

            // Assert
            Assert.True(AssessmentContext.Questions.Any(q => q.QuestionText == "Persist Test"));
        }

        // -------------------------------------------------------------
        // ID is assigned (EF Core behavior)
        // -------------------------------------------------------------
        [Fact]
        public async Task AddAsync_ShouldAssignDatabaseGeneratedId()
        {
            var question = _fixture.Build<Question>()
                .Without(q => q.Quiz)
                .Without(q => q.Answers)
                .With(q => q.QuestionText, "ID Test")
                .Create();

            await _repo.AddAsync(question);
            await _repo.SaveChangesAsync();

            Assert.True(question.Id > 0); // EF should assign PK
        }
    }
}
