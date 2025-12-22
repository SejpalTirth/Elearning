using AssessmentService.BLL.DTOs;
using FluentValidation;

namespace AssessmentService.BLL.Validators
{
    public class GetQuizForModuleDtoValidator : AbstractValidator<GetQuizForModuleDto>
    {
        public GetQuizForModuleDtoValidator()
        {
            RuleFor(x => x.ModuleId)
                .GreaterThan(0)
                .WithMessage("ModuleId must be greater than zero.");
        }
    }
}
