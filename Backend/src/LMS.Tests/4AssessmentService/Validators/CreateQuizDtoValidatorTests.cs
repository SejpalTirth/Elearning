using AutoFixture;
using DTOs._4AssessmentService;
using FluentValidation.TestHelper;
using AssessmentService.BLL.Validators;

namespace LMS.Tests._4AssessmentService.Validators
{
    public class CreateQuizDtoValidatorTests
    {
        private readonly IFixture _fixture;
        private readonly CreateQuizDtoValidator _validator;

        public CreateQuizDtoValidatorTests()
        {
            _fixture = new Fixture();
            _validator = new CreateQuizDtoValidator();
        }

        #region ModuleId tests

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Should_Have_Error_When_ModuleId_Is_Not_Positive(int invalidModuleId)
        {
            var dto = _fixture.Build<CreateQuizDto>()
                .With(x => x.ModuleId, invalidModuleId)
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.ModuleId)
                  .WithErrorMessage("ModuleId must be greater than zero.");
        }

        [Fact]
        public void Should_Not_Have_Error_When_ModuleId_Is_Positive()
        {
            var dto = _fixture.Build<CreateQuizDto>()
                .With(x => x.ModuleId, 1)
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.ModuleId);
        }

        #endregion

        #region Title tests

        [Fact]
        public void Should_Have_Error_When_Title_Is_Empty()
        {
            var dto = _fixture.Build<CreateQuizDto>()
                .With(x => x.Title, string.Empty)
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Title)
                  .WithErrorMessage("Quiz title is required.");
        }

        [Fact]
        public void Should_Not_Have_Error_When_Title_Is_Provided()
        {
            var dto = _fixture.Build<CreateQuizDto>()
                .With(x => x.Title, "Sample Quiz")
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.Title);
        }

        #endregion

        #region TimeLimitMinutes tests

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public void Should_Have_Error_When_TimeLimitMinutes_Is_Less_Than_Or_Equal_Zero(int invalidTime)
        {
            var dto = _fixture.Build<CreateQuizDto>()
                .With(x => x.TimeLimitMinutes, invalidTime)
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.TimeLimitMinutes)
                  .WithErrorMessage("TimeLimitMinutes must be greater than zero.");
        }

        [Theory]
        [InlineData(181)]
        [InlineData(500)]
        public void Should_Have_Error_When_TimeLimitMinutes_Exceeds_Max(int invalidTime)
        {
            var dto = _fixture.Build<CreateQuizDto>()
                .With(x => x.TimeLimitMinutes, invalidTime)
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.TimeLimitMinutes)
                  .WithErrorMessage("TimeLimitMinutes cannot exceed 180 minutes.");
        }

        [Theory]
        [InlineData(1)]
        [InlineData(60)]
        [InlineData(180)]
        public void Should_Not_Have_Error_When_TimeLimitMinutes_Is_Valid(int validTime)
        {
            var dto = _fixture.Build<CreateQuizDto>()
                .With(x => x.TimeLimitMinutes, validTime)
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.TimeLimitMinutes);
        }

        #endregion
    }
}
