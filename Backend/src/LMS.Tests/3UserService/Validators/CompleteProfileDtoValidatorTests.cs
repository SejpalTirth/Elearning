using AutoFixture;
using DTOs._3UserService;
using FluentValidation.TestHelper;
using global::UserService.BLL.Validators;

namespace LMS.Tests._3UserService.Validators
{
    public class CompleteProfileDtoValidatorTests
    {
        private readonly IFixture _fixture;
        private readonly CompleteProfileDtoValidator _validator;

        public CompleteProfileDtoValidatorTests()
        {
            _fixture = new Fixture();
            _validator = new CompleteProfileDtoValidator();
        }

        #region UserId tests

        [Fact]
        public void Should_Have_Error_When_UserId_Is_Null_Or_Empty()
        {
            var dto = _fixture.Build<CompleteProfileDto>()
                .With(x => x.UserId, string.Empty)
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.UserId)
                  .WithErrorMessage("UserId is required.");
        }

        [Fact]
        public void Should_Have_Error_When_UserId_Is_Not_Valid_Guid()
        {
            var dto = _fixture.Build<CompleteProfileDto>()
                .With(x => x.UserId, "invalid-guid")
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.UserId)
                  .WithErrorMessage("UserId must be a valid GUID.");
        }

        [Fact]
        public void Should_Not_Have_Error_When_UserId_Is_Valid_Guid()
        {
            var dto = _fixture.Build<CompleteProfileDto>()
                .With(x => x.UserId, Guid.NewGuid().ToString())
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.UserId);
        }

        #endregion

        #region Name tests

        [Fact]
        public void Should_Have_Error_When_Name_Is_Empty()
        {
            var dto = _fixture.Build<CompleteProfileDto>()
                .With(x => x.Name, string.Empty)
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Name)
                  .WithErrorMessage("Name is required.");
        }

        [Fact]
        public void Should_Not_Have_Error_When_Name_Is_Provided()
        {
            var dto = _fixture.Build<CompleteProfileDto>()
                .With(x => x.Name, "John Doe")
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.Name);
        }

        #endregion

        #region Role tests

        [Fact]
        public void Should_Have_Error_When_Role_Is_Empty()
        {
            var dto = _fixture.Build<CompleteProfileDto>()
                .With(x => x.Role, string.Empty)
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Role)
                  .WithErrorMessage("Role is required.");
        }

        [Fact]
        public void Should_Not_Have_Error_When_Role_Is_Provided()
        {
            var dto = _fixture.Build<CompleteProfileDto>()
                .With(x => x.Role, "Admin")
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.Role);
        }
        #endregion
    }
}
