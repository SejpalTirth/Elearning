using DTOs._2CourseService;
using FluentValidation;

namespace CourseService.BLL.Validators
{
    public class UpdateModuleDtoValidator : AbstractValidator<UpdateModuleDto>
    {
        public UpdateModuleDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("Module Id must be greater than zero.");

            RuleFor(x => x.Title)
                .NotEmpty()
                .When(x => x.Title != null)
                .WithMessage("Module title cannot be empty.");

            RuleFor(x => x.Content)
                .NotEmpty()
                .When(x => x.Content != null)
                .WithMessage("Module content cannot be empty.");
        }
    }
}
