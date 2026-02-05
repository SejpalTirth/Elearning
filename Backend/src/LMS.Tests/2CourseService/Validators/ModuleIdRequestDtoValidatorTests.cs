using AutoFixture;
using CourseService.BLL.Validators;
using DTOs._2CourseService;
using FluentValidation.TestHelper;

namespace LMS.Tests._2CourseService.Validators
{
    public class ModuleIdRequestDtoValidatorTests
    {
        private readonly IFixture _fixture;
        private readonly ModuleIdRequestDtoValidator _validator;

        public ModuleIdRequestDtoValidatorTests()
        {
            _fixture = new Fixture();
            _validator = new ModuleIdRequestDtoValidator();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-50)]
        public void Should_Have_Error_When_ModuleId_Is_Less_Than_Or_Equal_To_Zero(int invalidModuleId)
        {
            // Arrange
            var dto = _fixture.Build<ModuleIdRequestDto>()
                .With(x => x.ModuleId, invalidModuleId)
                .Create();

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ModuleId)
                  .WithErrorMessage("ModuleId must be greater than zero.");
        }

        [Fact]
        public void Should_Not_Have_Error_When_ModuleId_Is_Greater_Than_Zero()
        {
            // Arrange
            var dto = _fixture.Build<ModuleIdRequestDto>()
                .With(x => x.ModuleId, 1)
                .Create();

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.ModuleId);
        }
    }
}
