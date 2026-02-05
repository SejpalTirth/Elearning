using AutoFixture;
using DTOs._4AssessmentService;
using FluentValidation.TestHelper;
using AssessmentService.BLL.Validators;

namespace LMS.Tests._4AssessmentService.Validators
{
    public class GetQuizForModuleDtoValidatorTests
    {
        private readonly IFixture _fixture;
        private readonly GetQuizForModuleDtoValidator _validator;

        public GetQuizForModuleDtoValidatorTests()
        {
            _fixture = new Fixture();
            _validator = new GetQuizForModuleDtoValidator();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Should_Have_Error_When_ModuleId_Is_Not_Positive(int invalidModuleId)
        {
            var dto = _fixture.Build<GetQuizForModuleDto>()
                .With(x => x.ModuleId, invalidModuleId)
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.ModuleId)
                  .WithErrorMessage("ModuleId must be greater than zero.");
        }

        [Fact]
        public void Should_Not_Have_Error_When_ModuleId_Is_Positive()
        {
            var dto = _fixture.Build<GetQuizForModuleDto>()
                .With(x => x.ModuleId, 1)
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.ModuleId);
        }
    }
}
