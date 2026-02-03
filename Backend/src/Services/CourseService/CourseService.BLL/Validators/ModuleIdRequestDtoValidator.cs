using DTOs._2CourseService;
using FluentValidation;

namespace CourseService.BLL.Validators
{
    public class ModuleIdRequestDtoValidator : AbstractValidator<ModuleIdRequestDto>
    {
        public ModuleIdRequestDtoValidator()
        {
            RuleFor(x => x.ModuleId)
                .GreaterThan(0)
                .WithMessage("ModuleId must be greater than zero.");
        }
    }

}
