using DTOs._4AssessmentService;
using FluentValidation;

namespace AssessmentService.BLL.Validators
{
    public class CourseQuizStatusDtoValidator : AbstractValidator<CourseQuizStatusDto>
    {
        public CourseQuizStatusDtoValidator()
        {
            RuleFor(x => x.CourseId)
                .GreaterThan(0)
                .WithMessage("CourseId must be greater than zero.");
        }
    }
}
