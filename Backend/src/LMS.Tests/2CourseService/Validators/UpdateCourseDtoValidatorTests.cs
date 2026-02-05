using AutoFixture;
using CourseService.BLL.Validators;
using DTOs._2CourseService;
using FluentValidation.TestHelper;

namespace LMS.Tests._2CourseService.Validators
{
    public class UpdateCourseDtoValidatorTests
    {
        private readonly IFixture _fixture;
        private readonly UpdateCourseDtoValidator _validator;

        public UpdateCourseDtoValidatorTests()
        {
            _fixture = new Fixture();
            _validator = new UpdateCourseDtoValidator();
        }

        [Fact]
        public void Should_Have_Error_When_No_Fields_Are_Provided()
        {
            // Arrange
            var dto = new UpdateCourseDto();

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x)
                  .WithErrorMessage("At least one field must be provided for update.");
        }

        [Fact]
        public void Should_Pass_When_Title_Is_Provided()
        {
            var dto = new UpdateCourseDto
            {
                Title = _fixture.Create<string>()
            };

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x);
        }

        [Fact]
        public void Should_Pass_When_Description_Is_Provided()
        {
            var dto = new UpdateCourseDto
            {
                Description = _fixture.Create<string>()
            };

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x);
        }

        [Fact]
        public void Should_Pass_When_CategoryId_Is_Provided()
        {
            var dto = new UpdateCourseDto
            {
                CategoryId = 1
            };

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x);
        }

        [Fact]
        public void Should_Pass_When_Modules_Are_Provided()
        {
            var dto = new UpdateCourseDto
            {
                Modules = new List<UpdateModuleDto>
                {
                    _fixture.Create<UpdateModuleDto>()
                }
            };

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-10)]
        public void Should_Have_Error_When_CategoryId_Is_Negative(int invalidCategoryId)
        {
            var dto = new UpdateCourseDto
            {
                CategoryId = invalidCategoryId
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.CategoryId)
                  .WithErrorMessage("CategoryId must be greater than zero.");
        }

        [Fact]
        public void Should_Not_Have_Error_When_CategoryId_Is_Zero()
        {
            var dto = new UpdateCourseDto
            {
                CategoryId = 0
            };

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.CategoryId);
        }

        [Fact]
        public void Should_Have_Error_When_Module_Is_Invalid()
        {
            var invalidModule = new UpdateModuleDto
            {
                Title = string.Empty
            };

            var dto = new UpdateCourseDto
            {
                Modules = new List<UpdateModuleDto> { invalidModule }
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor("Modules[0].Title");
        }
    }
}
