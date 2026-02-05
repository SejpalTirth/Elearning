using AutoFixture;
using DTOs._3UserService;
using FluentValidation.TestHelper;
using global::UserService.BLL.Validators;

namespace LMS.Tests._3UserService.Validators
{
    public class UpdateUserRoleRequestValidatorTests
    {
        private readonly IFixture _fixture;
        private readonly UpdateUserRoleRequestValidator _validator;

        public UpdateUserRoleRequestValidatorTests()
        {
            _fixture = new Fixture();
            _validator = new UpdateUserRoleRequestValidator();
        }

        #region UserId tests

        [Fact]
        public void Should_Have_Error_When_UserId_Is_Empty()
        {
            var dto = _fixture.Build<UpdateUserRoleRequest>()
                .With(x => x.UserId, Guid.Empty)
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.UserId)
                  .WithErrorMessage("UserId is required.");
        }

        [Fact]
        public void Should_Not_Have_Error_When_UserId_Is_Provided()
        {
            var dto = _fixture.Build<UpdateUserRoleRequest>()
            .With(x => x.UserId, Guid.NewGuid())
            .Create();


            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.UserId);
        }

        #endregion

        #region RoleId tests

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Should_Have_Error_When_RoleId_Is_Not_Positive(int invalidRoleId)
        {
            var dto = _fixture.Build<UpdateUserRoleRequest>()
                .With(x => x.RoleId, invalidRoleId)
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.RoleId)
                  .WithErrorMessage("RoleId must be greater than zero.");
        }

        [Fact]
        public void Should_Not_Have_Error_When_RoleId_Is_Positive()
        {
            var dto = _fixture.Build<UpdateUserRoleRequest>()
                .With(x => x.RoleId, 1)
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.RoleId);
        }

        #endregion
    }
}
