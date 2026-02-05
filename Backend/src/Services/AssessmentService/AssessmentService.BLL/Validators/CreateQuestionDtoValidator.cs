using DTOs._4AssessmentService;
using FluentValidation;

namespace AssessmentService.BLL.Validators
{
    public class CreateQuestionDtoValidator
        : AbstractValidator<CreateQuestionDto>
    {
        public CreateQuestionDtoValidator()
        {
            RuleFor(x => x.Question)
                .NotEmpty()
                .WithMessage("Question is required.");

            RuleFor(x => x.Marks)
                .GreaterThan(0)
                .WithMessage("Marks must be greater than zero.");

            RuleFor(x => x.Options)
                .Must(o => o != null && o.Count == 4)
                .WithMessage("Exactly 4 options are required.");


            RuleForEach(x => x.Options)
                .NotEmpty()
                .WithMessage("Option text cannot be empty.");

            RuleFor(x => x.CorrectAnswerIndex)
                .InclusiveBetween(0, 3)
                .WithMessage("CorrectAnswerIndex must be between 0 and 3.");
        }
    }
}
