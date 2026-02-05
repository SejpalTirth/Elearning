using AssessmentService.BLL.Validators;
using AutoFixture;
using DTOs._4AssessmentService;
using FluentValidation.TestHelper;

namespace LMS.Tests._4AssessmentService.Validators
{
    public class SubmitAnswerDtoValidatorTests
    {
        private readonly IFixture _fixture;
        private readonly SubmitAnswerDtoValidator _validator;

        public SubmitAnswerDtoValidatorTests()
        {
            _fixture = new Fixture();
            _validator = new SubmitAnswerDtoValidator();
        }

        #region QuestionId tests

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Should_Have_Error_When_QuestionId_Is_Not_Positive(int invalidId)
        {
            var dto = _fixture.Build<SubmitAnswerDto>()
                .With(x => x.QuestionId, invalidId)
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.QuestionId)
                  .WithErrorMessage("QuestionId must be greater than zero.");
        }

        [Fact]
        public void Should_Not_Have_Error_When_QuestionId_Is_Positive()
        {
            var dto = _fixture.Build<SubmitAnswerDto>()
                .With(x => x.QuestionId, 1)
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.QuestionId);
        }

        #endregion

        #region SelectedAnswerId tests

        [Theory]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Should_Have_Error_When_SelectedAnswerId_Is_Negative(int invalidIndex)
        {
            var dto = _fixture.Build<SubmitAnswerDto>()
                .With(x => x.SelectedAnswerId, invalidIndex)
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.SelectedAnswerId)
                  .WithErrorMessage("SelectedAnswerId must be a valid option index.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        public void Should_Not_Have_Error_When_SelectedAnswerId_Is_Zero_Or_Positive(int validIndex)
        {
            var dto = _fixture.Build<SubmitAnswerDto>()
                .With(x => x.SelectedAnswerId, validIndex)
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.SelectedAnswerId);
        }

        #endregion
    }
}
