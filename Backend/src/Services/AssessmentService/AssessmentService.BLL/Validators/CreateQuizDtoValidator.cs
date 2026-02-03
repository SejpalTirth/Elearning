using DTOs._4AssessmentService;
using FluentValidation;

namespace AssessmentService.BLL.Validators
{
    public class CreateQuizDtoValidator
        : AbstractValidator<CreateQuizDto>
    {
        public CreateQuizDtoValidator()
        {
            RuleFor(x => x.ModuleId)
                .GreaterThan(0)
                .WithMessage("ModuleId must be greater than zero.");

            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Quiz title is required.");

            RuleFor(x => x.TimeLimitMinutes)
                .GreaterThan(0)
                .WithMessage("TimeLimitMinutes must be greater than zero.")
                .LessThanOrEqualTo(180)
                .WithMessage("TimeLimitMinutes cannot exceed 180 minutes.");
        }
    }
}
