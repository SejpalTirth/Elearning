using AutoFixture;
using CourseService.BLL.Validators;
using DTOs._2CourseService;
using FluentValidation.TestHelper;

namespace LMS.Tests._2CourseService.Validators
{
    public class CourseDtoValidatorTests
    {
        private readonly IFixture _fixture;
        private readonly CourseDtoValidator _validator;

        public CourseDtoValidatorTests()
        {
            _fixture = new Fixture();
            _validator = new CourseDtoValidator();
        }

        [Fact]
        public void Should_Have_Error_When_Title_Is_Empty()
        {
            var dto = _fixture.Build<CourseDto>()
                .With(x => x.Title, string.Empty)
                .With(x => x.Modules, new List<ModuleDto>
                    {
                        _fixture.Create<ModuleDto>()
                    })
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Title)
                  .WithErrorMessage("Course title is required.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Should_Have_Error_When_CategoryId_Is_Invalid(int categoryId)
        {
            var dto = _fixture.Build<CourseDto>()
                .With(x => x.CategoryId, categoryId)
                .With(x => x.Modules, new List<ModuleDto>
                    {
                        _fixture.Create<ModuleDto>()
                    })
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.CategoryId)
                  .WithErrorMessage("CategoryId must be greater than zero.");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Should_Have_Error_When_InstructorUserId_Is_Empty(string userId)
        {
            var dto = _fixture.Build<CourseDto>()
                .With(x => x.InstructorUserId, userId)
                .With(x => x.Modules, new List<ModuleDto> { _fixture.Create<ModuleDto>() })
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.InstructorUserId)
                  .WithErrorMessage("InstructorUserId is required.");
        }

        [Fact]
        public void Should_Have_Error_When_InstructorUserId_Is_Not_Valid_Guid()
        {
            var dto = _fixture.Build<CourseDto>()
                .With(x => x.InstructorUserId, "not-a-guid")
                .With(x => x.Modules, new List<ModuleDto>
                    {
                        _fixture.Create<ModuleDto>()
                    })
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.InstructorUserId)
                  .WithErrorMessage("InstructorUserId must be a valid GUID.");
        }

        [Fact]
        public void Should_Have_Error_When_Modules_Is_Null()
        {
            var dto = _fixture.Build<CourseDto>()
                .With(x => x.Modules, (ICollection<ModuleDto>)null!)
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Modules)
                  .WithErrorMessage("At least one module is required.");
        }

        [Fact]
        public void Should_Have_Error_When_Modules_Is_Empty()
        {
            var dto = _fixture.Build<CourseDto>()
                .With(x => x.Modules, new List<ModuleDto>())
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Modules)
                  .WithErrorMessage("At least one module is required.");
        }

        [Fact]
        public void Should_Pass_When_All_Fields_Are_Valid()
        {
            var dto = _fixture.Build<CourseDto>()
                .With(x => x.Title, _fixture.Create<string>())
                .With(x => x.CategoryId, 1)
                .With(x => x.InstructorUserId, Guid.NewGuid().ToString())
                .With(x => x.Modules, new List<ModuleDto>
                    {
                        _fixture.Create<ModuleDto>()
                    })
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
