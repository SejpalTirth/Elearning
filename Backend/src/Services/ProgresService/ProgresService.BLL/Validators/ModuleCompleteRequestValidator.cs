using FluentValidation;
using DTOs._5ProgresService;

namespace ProgressService.BLL.Validators
{
    public class ModuleCompleteRequestValidator
        : AbstractValidator<ModuleCompleteRequest>
    {
        public ModuleCompleteRequestValidator()
        {
            RuleFor(x => x.ModuleId)
                .GreaterThan(0)
                .WithMessage("ModuleId must be greater than zero.");
        }
    }
}
