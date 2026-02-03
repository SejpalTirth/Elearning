using DTOs._2CourseService;
using FluentValidation;

namespace CourseService.BLL.Validators
{
    public class EnrollRequestDtoValidator : AbstractValidator<EnrollRequestDto>
    {
        public EnrollRequestDtoValidator()
        {
            RuleFor(x => x.CourseId)
                .GreaterThan(0)
                .WithMessage("CourseId must be greater than zero.");
        }
    }
}
