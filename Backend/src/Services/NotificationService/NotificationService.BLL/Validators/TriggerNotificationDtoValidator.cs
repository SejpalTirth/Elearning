using FluentValidation;
using DTOs._6NotificationService;

namespace NotificationService.BLL.Validators
{
    public class TriggerNotificationDtoValidator
        : AbstractValidator<TriggerNotificationDto>
    {
        public TriggerNotificationDtoValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("UserId is required.");

            RuleFor(x => x.Type)
                .IsInEnum()
                .WithMessage("Invalid notification type.");

            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required.");

            RuleFor(x => x.Data)
                .NotEmpty()
                .WithMessage("Notification data is required.");
        }
    }
}
