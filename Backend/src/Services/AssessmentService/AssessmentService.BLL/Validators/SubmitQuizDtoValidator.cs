using DTOs._4AssessmentService;
using FluentValidation;

namespace AssessmentService.BLL.Validators
{
    public class SubmitQuizDtoValidator : AbstractValidator<SubmitQuizDto>
    {
        public SubmitQuizDtoValidator()
        {
            RuleFor(x => x.UserId)
                .NotEqual(Guid.Empty)
                .WithMessage("UserId is required.");

            RuleFor(x => x.QuizId)
                .GreaterThan(0)
                .WithMessage("QuizId must be greater than zero.");

            RuleFor(x => x.Answers)
                .NotEmpty()
                .WithMessage("At least one answer must be submitted.");

            RuleForEach(x => x.Answers)
                .SetValidator(new SubmitAnswerDtoValidator());
        }
    }
}
