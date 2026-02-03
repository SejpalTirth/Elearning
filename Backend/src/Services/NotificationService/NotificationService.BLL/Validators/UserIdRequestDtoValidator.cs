using FluentValidation;
using DTOs._6NotificationService;

namespace NotificationService.BLL.Validators
{
    public class UserIdRequestDtoValidator
        : AbstractValidator<UserIdRequestDto>
    {
        public UserIdRequestDtoValidator()
        {
            RuleFor(x => x.UserId)
            .NotEqual(Guid.Empty)
            .WithMessage("Please enter a valid GUID for submission.");

        }
    }
}
