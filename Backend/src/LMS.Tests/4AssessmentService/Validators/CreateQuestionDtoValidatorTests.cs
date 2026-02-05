using AssessmentService.BLL.Validators;
using AutoFixture;
using DTOs._4AssessmentService;
using FluentValidation.TestHelper;

namespace LMS.Tests._4AssessmentService.Validators
{
    public class CreateQuestionDtoValidatorTests
    {
        private readonly IFixture _fixture;
        private readonly CreateQuestionDtoValidator _validator;

        public CreateQuestionDtoValidatorTests()
        {
            _fixture = new Fixture();
            _validator = new CreateQuestionDtoValidator();
        }

        #region Question tests

        [Fact]
        public void Should_Have_Error_When_Question_Is_Empty()
        {
            var dto = _fixture.Build<CreateQuestionDto>()
                .With(x => x.Question, string.Empty)
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Question)
                  .WithErrorMessage("Question is required.");
        }

        [Fact]
        public void Should_Not_Have_Error_When_Question_Is_Provided()
        {
            var dto = _fixture.Build<CreateQuestionDto>()
                .With(x => x.Question, "Sample question?")
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.Question);
        }

        #endregion

        #region Marks tests

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void Should_Have_Error_When_Marks_Is_Not_Positive(int invalidMarks)
        {
            var dto = _fixture.Build<CreateQuestionDto>()
                .With(x => x.Marks, invalidMarks)
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Marks)
                  .WithErrorMessage("Marks must be greater than zero.");
        }

        [Fact]
        public void Should_Not_Have_Error_When_Marks_Is_Positive()
        {
            var dto = _fixture.Build<CreateQuestionDto>()
                .With(x => x.Marks, 5)
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.Marks);
        }

        #endregion

        #region Options tests

        [Fact]
        public void Should_Have_Error_When_Options_Is_Null()
        {
            var dto = _fixture.Build<CreateQuestionDto>()
                .With(x => x.Options, (List<string>)null)
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Options)
                  .WithErrorMessage("Exactly 4 options are required.");
        }

        [Fact]
        public void Should_Have_Error_When_Options_Count_Is_Not_4()
        {
            var dto = _fixture.Build<CreateQuestionDto>()
                .With(x => x.Options, new List<string> { "A", "B", "C" }) // 3 options
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Options)
                  .WithErrorMessage("Exactly 4 options are required.");
        }

        [Fact]
        public void Should_Have_Error_When_Any_Option_Is_Empty()
        {
            var dto = _fixture.Build<CreateQuestionDto>()
                .With(x => x.Options, new List<string> { "A", "", "C", "D" })
                .Create();

            var result = _validator.TestValidate(dto);

            // Should report error on Options[1]
            result.ShouldHaveValidationErrorFor("Options[1]")
                  .WithErrorMessage("Option text cannot be empty.");
        }

        [Fact]
        public void Should_Not_Have_Error_When_All_Options_Are_Valid()
        {
            var dto = _fixture.Build<CreateQuestionDto>()
                .With(x => x.Options, new List<string> { "A", "B", "C", "D" })
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.Options);
        }

        #endregion

        #region CorrectAnswerIndex tests

        [Theory]
        [InlineData(-1)]
        [InlineData(4)]
        [InlineData(10)]
        public void Should_Have_Error_When_CorrectAnswerIndex_Is_Out_Of_Range(int invalidIndex)
        {
            var dto = _fixture.Build<CreateQuestionDto>()
                .With(x => x.CorrectAnswerIndex, invalidIndex)
                .With(x => x.Options, new List<string> { "A", "B", "C", "D" })
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.CorrectAnswerIndex)
                  .WithErrorMessage("CorrectAnswerIndex must be between 0 and 3.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void Should_Not_Have_Error_When_CorrectAnswerIndex_Is_Valid(int validIndex)
        {
            var dto = _fixture.Build<CreateQuestionDto>()
                .With(x => x.CorrectAnswerIndex, validIndex)
                .With(x => x.Options, new List<string> { "A", "B", "C", "D" })
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.CorrectAnswerIndex);
        }

        #endregion
    }

}
