using CourseService.BLL.DTOs;
using FluentValidation;

namespace CourseService.BLL.Validators
{
    public class CourseIdRequestDtoValidator : AbstractValidator<CourseIdRequestDto>
    {
        public CourseIdRequestDtoValidator()
        {
            RuleFor(x => x.CourseId)
                .GreaterThan(0)
                .WithMessage("CourseId must be greater than zero.");
        }
    }
}
