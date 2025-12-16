using FluentValidation;
using UserService.BLL.DTOs;

namespace UserService.BLL.Validators
{
    public class CompleteProfileDtoValidator
        : AbstractValidator<CompleteProfileDto>
    {
        public CompleteProfileDtoValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("UserId is required.")
                .Must(BeValidGuid)
                .WithMessage("UserId must be a valid GUID.");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Name is required.");

            RuleFor(x => x.Role)
                .NotEmpty()
                .WithMessage("Role is required.");
        }

        private bool BeValidGuid(string userId)
        {
            return Guid.TryParse(userId, out _);
        }
    }
}
