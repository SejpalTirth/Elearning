using AutoFixture;
using CourseService.BLL.Validators;
using DTOs._2CourseService;
using FluentValidation.TestHelper;

namespace LMS.Tests._2CourseService.Validators
{
    public class UpdateModuleDtoValidatorTests
    {
        private readonly IFixture _fixture;
        private readonly UpdateModuleDtoValidator _validator;

        public UpdateModuleDtoValidatorTests()
        {
            _fixture = new Fixture();
            _validator = new UpdateModuleDtoValidator();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Should_Have_Error_When_Id_Is_Not_Positive(int invalidId)
        {
            var dto = _fixture.Build<UpdateModuleDto>()
                .With(x => x.Id, invalidId)
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Module Id must be greater than zero.");
        }

        [Fact]
        public void Should_Not_Have_Error_When_Id_Is_Positive()
        {
            var dto = _fixture.Build<UpdateModuleDto>()
                .With(x => x.Id, 1)
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public void Should_Not_Have_Error_When_Title_Is_Null()
        {
            var dto = _fixture.Build<UpdateModuleDto>()
                .With(x => x.Title, (string)null!)
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.Title);
        }

        [Fact]
        public void Should_Have_Error_When_Title_Is_Empty()
        {
            var dto = _fixture.Build<UpdateModuleDto>()
                .With(x => x.Title, string.Empty)
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Title)
                  .WithErrorMessage("Module title cannot be empty.");
        }

        [Fact]
        public void Should_Not_Have_Error_When_Title_Is_Not_Empty()
        {
            var dto = _fixture.Build<UpdateModuleDto>()
                .With(x => x.Title, "Module 1")
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.Title);
        }

        [Fact]
        public void Should_Not_Have_Error_When_Content_Is_Null()
        {
            var dto = _fixture.Build<UpdateModuleDto>()
                .With(x => x.Content, (string)null!)
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.Content);
        }

        [Fact]
        public void Should_Have_Error_When_Content_Is_Empty()
        {
            var dto = _fixture.Build<UpdateModuleDto>()
                .With(x => x.Content, string.Empty)
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Content)
                  .WithErrorMessage("Module content cannot be empty.");
        }

        [Fact]
        public void Should_Not_Have_Error_When_Content_Is_Not_Empty()
        {
            var dto = _fixture.Build<UpdateModuleDto>()
                .With(x => x.Content, "Some content")
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.Content);
        }
    }
}
