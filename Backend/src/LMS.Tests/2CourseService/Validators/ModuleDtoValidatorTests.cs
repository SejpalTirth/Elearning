using AutoFixture;
using CourseService.BLL.Validators;
using DTOs._2CourseService;
using FluentValidation.TestHelper;

namespace LMS.Tests._2CourseService.Validators
{
    public class ModuleDtoValidatorTests
    {
        private readonly IFixture _fixture;
        private readonly ModuleDtoValidator _validator;

        public ModuleDtoValidatorTests()
        {
            _fixture = new Fixture();
            _validator = new ModuleDtoValidator();
        }

        [Fact]
        public void Should_Have_Error_When_Title_Is_Empty()
        {
            // Arrange
            var dto = _fixture.Build<ModuleDto>()
                .With(x => x.Title, string.Empty)
                .With(x => x.Content, _fixture.Create<string>())
                .Create();

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Title)
                  .WithErrorMessage("Module title is required.");
        }

        [Fact]
        public void Should_Have_Error_When_Content_Is_Empty()
        {
            // Arrange
            var dto = _fixture.Build<ModuleDto>()
                .With(x => x.Content, string.Empty)
                .With(x => x.Title, _fixture.Create<string>())
                .Create();

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Content)
                  .WithErrorMessage("Module content is required.");
        }

        [Fact]
        public void Should_Pass_When_Title_And_Content_Are_Provided()
        {
            // Arrange
            var dto = _fixture.Build<ModuleDto>()
                .With(x => x.Title, _fixture.Create<string>())
                .With(x => x.Content, _fixture.Create<string>())
                .Create();

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }

}
