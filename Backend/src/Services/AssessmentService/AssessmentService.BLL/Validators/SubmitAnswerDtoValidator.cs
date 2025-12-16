using AssessmentService.BLL.DTOs;
using FluentValidation;

namespace AssessmentService.BLL.Validators
{
    public class SubmitAnswerDtoValidator
        : AbstractValidator<SubmitAnswerDto>
    {
        public SubmitAnswerDtoValidator()
        {
            RuleFor(x => x.QuestionId)
                .GreaterThan(0)
                .WithMessage("QuestionId must be greater than zero.");

            RuleFor(x => x.SelectedAnswerId)
                .GreaterThanOrEqualTo(0)
                .WithMessage("SelectedAnswerId must be a valid option index.");
        }
    }
}
