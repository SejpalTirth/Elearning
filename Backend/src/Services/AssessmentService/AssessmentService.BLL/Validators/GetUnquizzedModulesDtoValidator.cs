using AssessmentService.BLL.DTOs;
using FluentValidation;

namespace AssessmentService.BLL.Validators
{
    public class GetUnquizzedModulesDtoValidator : AbstractValidator<GetUnquizzedModulesDto>
    {
        public GetUnquizzedModulesDtoValidator()
        {
            RuleFor(x => x.CourseId)
                .GreaterThan(0)
                .WithMessage("CourseId must be greater than zero.");
        }
    }
}
