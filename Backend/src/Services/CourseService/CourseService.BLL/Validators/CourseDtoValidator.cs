    using DTOs._2CourseService;
using FluentValidation;

namespace CourseService.BLL.Validators
{
    public class CourseDtoValidator : AbstractValidator<CourseDto>
    {
        public CourseDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Course title is required.");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0)
                .WithMessage("CategoryId must be greater than zero.");

            RuleFor(x => x.InstructorUserId)
                .NotEmpty()
                .WithMessage("InstructorUserId is required.")
                .Must(BeValidGuid)
                .WithMessage("InstructorUserId must be a valid GUID.");

            RuleFor(x => x.Modules)
                .NotNull()
                .Must(m => m != null && m.Any())
                .WithMessage("At least one module is required.");

            RuleForEach(x => x.Modules)
                .SetValidator(new ModuleDtoValidator());
        }

        private bool BeValidGuid(string userId)
        {
            return Guid.TryParse(userId, out _);
        }
    }
}
