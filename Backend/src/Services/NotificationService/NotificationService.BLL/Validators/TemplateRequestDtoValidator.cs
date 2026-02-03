using FluentValidation;
using DTOs._6NotificationService;

namespace NotificationService.BLL.Validators
{
    public class TemplateRequestDtoValidator
        : AbstractValidator<TemplateRequestDTO>
    {
        public TemplateRequestDtoValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("UserId is required.");

            RuleFor(x => x.TemplateName)
                .NotEmpty()
                .WithMessage("TemplateName is required.");

            RuleFor(x => x.Model)
                .NotNull()
                .WithMessage("Template model is required.");
        }
    }
}
