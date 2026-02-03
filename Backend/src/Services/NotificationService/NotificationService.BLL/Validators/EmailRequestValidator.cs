using FluentValidation;
using DTOs._6NotificationService;

namespace NotificationService.BLL.Validators
{
    public class EmailRequestValidator
        : AbstractValidator<EmailRequestDTO>
    {
        public EmailRequestValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("UserId is required.");

            RuleFor(x => x.Subject)
                .NotEmpty()
                .WithMessage("Email subject is required.");

            RuleFor(x => x.Body)
                .NotEmpty()
                .WithMessage("Email body is required.");
        }
    }
}
