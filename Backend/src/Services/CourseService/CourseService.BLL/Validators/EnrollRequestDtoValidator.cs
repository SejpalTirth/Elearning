using CourseService.BLL.DTOs;
using FluentValidation;

namespace CourseService.BLL.Validators
{
    public class EnrollRequestDtoValidator : AbstractValidator<EnrollRequestDto>
    {
        public EnrollRequestDtoValidator()
        {
            RuleFor(x => x.CourseId)
                .GreaterThan(0)
                .WithMessage("CourseId must be greater than zero.");

            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("UserId is required.")
                .Must(BeValidGuid)
                .WithMessage("UserId must be a valid GUID.");
        }

        private bool BeValidGuid(string userId)
        {
            return Guid.TryParse(userId, out _);
        }
    }
}
