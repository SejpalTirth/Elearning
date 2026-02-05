using AssessmentService.BLL.Validators;
using AutoFixture;
using DTOs._4AssessmentService;
using FluentValidation.TestHelper;

namespace LMS.Tests._4AssessmentService.Validators
{



    public class CourseQuizStatusDtoValidatorTests
    {
        private readonly IFixture _fixture;
        private readonly CourseQuizStatusDtoValidator _validator;

        public CourseQuizStatusDtoValidatorTests()
        {
            _fixture = new Fixture();
            _validator = new CourseQuizStatusDtoValidator();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Should_Have_Error_When_CourseId_Is_Not_Positive(int invalidCourseId)
        {
            var dto = _fixture.Build<CourseQuizStatusDto>()
                .With(x => x.CourseId, invalidCourseId)
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.CourseId)
                  .WithErrorMessage("CourseId must be greater than zero.");
        }

        [Fact]
        public void Should_Not_Have_Error_When_CourseId_Is_Positive()
        {
            var dto = _fixture.Build<CourseQuizStatusDto>()
                .With(x => x.CourseId, 1)
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.CourseId);
        }
    }
}
