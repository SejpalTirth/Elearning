using AutoFixture;
using DTOs._5ProgresService;
using FluentValidation.TestHelper;
using ProgressService.BLL.Validators;

namespace LMS.Tests._5ProgressService.Validators
{
    public class ModuleCompleteRequestValidatorTests
    {
        private readonly IFixture _fixture;
        private readonly ModuleCompleteRequestValidator _validator;

        public ModuleCompleteRequestValidatorTests()
        {
            _fixture = new Fixture();
            _validator = new ModuleCompleteRequestValidator();
        }

        [Fact]
        public void Should_Have_Error_When_ModuleId_Is_Zero()
        {
            var dto = _fixture.Build<ModuleCompleteRequest>()
                .With(x => x.ModuleId, 0)
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.ModuleId)
                  .WithErrorMessage("ModuleId must be greater than zero.");
        }

        [Fact]
        public void Should_Have_Error_When_ModuleId_Is_Negative()
        {
            var dto = _fixture.Build<ModuleCompleteRequest>()
                .With(x => x.ModuleId, -5)
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.ModuleId)
                  .WithErrorMessage("ModuleId must be greater than zero.");
        }

        [Fact]
        public void Should_Not_Have_Error_When_ModuleId_Is_Positive()
        {
            var dto = _fixture.Build<ModuleCompleteRequest>()
                .With(x => x.ModuleId, 1)
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.ModuleId);
        }
    }
}
