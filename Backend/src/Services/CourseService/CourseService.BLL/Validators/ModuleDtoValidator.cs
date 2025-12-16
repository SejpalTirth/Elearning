using CourseService.BLL.DTOs;
using FluentValidation;

namespace CourseService.BLL.Validators
{
    public class ModuleDtoValidator : AbstractValidator<ModuleDto>
    {
        public ModuleDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Module title is required.");

            RuleFor(x => x.Content)
                .NotEmpty()
                .WithMessage("Module content is required.");
        }
    }
}
