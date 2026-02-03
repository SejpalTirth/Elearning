using DTOs._2CourseService;
using FluentValidation;

namespace CourseService.BLL.Validators
{
    public class UpdateCourseDtoValidator : AbstractValidator<UpdateCourseDto>
    {
        public UpdateCourseDtoValidator()
        {
            RuleFor(x => x)
                .Must(HaveAtLeastOneField)
                .WithMessage("At least one field must be provided for update.");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0)
                .When(x => x.CategoryId != 0)
                .WithMessage("CategoryId must be greater than zero.");

            RuleForEach(x => x.Modules)
                .SetValidator(new UpdateModuleDtoValidator())
                .When(x => x.Modules != null && x.Modules.Any());
        }

        private bool HaveAtLeastOneField(UpdateCourseDto dto)
        {
            return
                !string.IsNullOrWhiteSpace(dto.Title) ||
                !string.IsNullOrWhiteSpace(dto.Description) ||
                dto.CategoryId > 0 ||
                (dto.Modules != null && dto.Modules.Any());
        }
    }
}
