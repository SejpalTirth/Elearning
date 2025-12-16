using FluentValidation;
using UserService.BLL.DTOs;

namespace UserService.BLL.Validators
{
    public class UpdateUserRoleRequestValidator
        : AbstractValidator<UpdateUserRoleRequest>
    {
        public UpdateUserRoleRequestValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("UserId is required.");

            RuleFor(x => x.RoleId)
                .GreaterThan(0)
                .WithMessage("RoleId must be greater than zero.");
        }
    }
}
