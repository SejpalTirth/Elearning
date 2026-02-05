using AutoFixture;
using DTOs._6NotificationService;
using FluentValidation.TestHelper;
using NotificationService.BLL.Validators;

namespace LMS.Tests._6NotificationService.Validators
{
    public class UserIdRequestDtoValidatorTests
    {
        private readonly UserIdRequestDtoValidator _validator;
        private readonly Fixture _fixture;

        public UserIdRequestDtoValidatorTests()
        {
            _validator = new UserIdRequestDtoValidator();
            _fixture = new Fixture();
        }

        [Fact]
        public void Should_Have_Error_When_UserId_Is_EmptyGuid()
        {
            // Arrange
            var dto = _fixture.Build<UserIdRequestDto>()
                .With(x => x.UserId, Guid.Empty)
                .Create();

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.UserId)
                .WithErrorMessage("Please enter a valid GUID for submission.");
        }

        [Fact]
        public void Should_Not_Have_Error_When_UserId_Is_ValidGuid()
        {
            // Arrange
            var dto = _fixture.Build<UserIdRequestDto>()
                .With(x => x.UserId, Guid.NewGuid())
                .Create();

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.UserId);
        }
    }
}
