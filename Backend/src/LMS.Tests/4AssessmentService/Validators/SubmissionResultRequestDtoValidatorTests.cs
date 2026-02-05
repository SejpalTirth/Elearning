using AutoFixture;
using DTOs._4AssessmentService;
using FluentValidation.TestHelper;
using AssessmentService.BLL.Validators;

namespace LMS.Tests._4AssessmentService.Validators
{
    public class SubmissionResultRequestDtoValidatorTests
    {
        private readonly IFixture _fixture;
        private readonly SubmissionResultRequestDtoValidator _validator;

        public SubmissionResultRequestDtoValidatorTests()
        {
            _fixture = new Fixture();
            _validator = new SubmissionResultRequestDtoValidator();
        }

        [Fact]
        public void Should_Have_Error_When_SubmissionId_Is_EmptyGuid()
        {
            var dto = _fixture.Build<SubmissionResultRequestDto>()
                .With(x => x.SubmissionId, Guid.Empty)
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.SubmissionId)
                  .WithErrorMessage("Please enter a valid GUID for submission.");
        }

        [Fact]
        public void Should_Not_Have_Error_When_SubmissionId_Is_ValidGuid()
        {
            var dto = _fixture.Build<SubmissionResultRequestDto>()
                .With(x => x.SubmissionId, Guid.NewGuid())
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.SubmissionId);
        }
    }
}
