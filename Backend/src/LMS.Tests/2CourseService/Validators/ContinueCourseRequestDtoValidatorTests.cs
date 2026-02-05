using AutoFixture;
using CourseService.BLL.Validators;
using DTOs._2CourseService;
using FluentValidation.TestHelper;

namespace LMS.Tests._2CourseService.Validators
{
        public class ContinueCourseRequestDtoValidatorTests
        {
            private readonly IFixture _fixture;
            private readonly ContinueCourseRequestDtoValidator _validator;

            public ContinueCourseRequestDtoValidatorTests()
            {
                _fixture = new Fixture();
                _validator = new ContinueCourseRequestDtoValidator();
            }

            [Theory]
            [InlineData(0)]
            [InlineData(-1)]
            [InlineData(-100)]
            public void Should_Have_Error_When_CourseId_Is_Less_Than_Or_Equal_To_Zero(int invalidCourseId)
            {
                // Arrange
                var dto = _fixture.Build<ContinueCourseRequestDto>()
                    .With(x => x.CourseId, invalidCourseId)
                    .Create();

                // Act
                var result = _validator.TestValidate(dto);

                // Assert
                result.ShouldHaveValidationErrorFor(x => x.CourseId)
                      .WithErrorMessage("CourseId must be greater than zero.");
            }

            [Fact]
            public void Should_Not_Have_Error_When_CourseId_Is_Greater_Than_Zero()
            {
                // Arrange
                var dto = _fixture.Build<ContinueCourseRequestDto>()
                    .With(x => x.CourseId, 1)
                    .Create();

                // Act
                var result = _validator.TestValidate(dto);

                // Assert
                result.ShouldNotHaveValidationErrorFor(x => x.CourseId);
            }
        }
}
