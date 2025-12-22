using AssessmentService.BLL.DTOs;
using FluentValidation;

namespace AssessmentService.BLL.Validators
{
    public class SubmissionResultRequestDtoValidator
        : AbstractValidator<SubmissionResultRequestDto>
    {
        public SubmissionResultRequestDtoValidator()
        {
            RuleFor(x => x.SubmissionId)
            .NotEqual(Guid.Empty)
            .WithMessage("Please enter a valid GUID for submission.");

        }
    }
}
