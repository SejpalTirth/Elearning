using CourseService.BLL.DTOs;
using FluentValidation;

namespace CourseService.BLL.Validators
{
    public class RestoreCourseRequestDtoValidator : AbstractValidator<RestoreCourseRequestDto>
    {
        public RestoreCourseRequestDtoValidator()
        {
            RuleFor(x => x.CourseId)
                .GreaterThan(0)
                .WithMessage("CourseId must be greater than zero.");
        }
    }
}
