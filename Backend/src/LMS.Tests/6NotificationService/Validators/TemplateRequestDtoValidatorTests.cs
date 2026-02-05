using AutoFixture;
using DTOs._6NotificationService;
using FluentValidation.TestHelper;
using NotificationService.BLL.Validators;

namespace LMS.Tests._6NotificationService.Validators
{
    public class TemplateRequestDtoValidatorTests
    {
        private readonly IFixture _fixture;
        private readonly TemplateRequestDtoValidator _validator;

        public TemplateRequestDtoValidatorTests()
        {
            _fixture = new Fixture();
            _validator = new TemplateRequestDtoValidator();
        }

        #region UserId tests

        [Fact]
        public void Should_Have_Error_When_UserId_Is_Empty()
        {
            var dto = _fixture.Build<TemplateRequestDTO>()
                .With(x => x.UserId, Guid.Empty)
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.UserId)
                  .WithErrorMessage("UserId is required.");
        }

        [Fact]
        public void Should_Not_Have_Error_When_UserId_Is_Provided()
        {
            var dto = _fixture.Build<TemplateRequestDTO>()
                .With(x => x.UserId, Guid.NewGuid())
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.UserId);
        }

        #endregion

        #region TemplateName tests

        [Fact]
        public void Should_Have_Error_When_TemplateName_Is_Empty()
        {
            var dto = _fixture.Build<TemplateRequestDTO>()
                .With(x => x.TemplateName, string.Empty)
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.TemplateName)
                  .WithErrorMessage("TemplateName is required.");
        }

        [Fact]
        public void Should_Not_Have_Error_When_TemplateName_Is_Provided()
        {
            var dto = _fixture.Build<TemplateRequestDTO>()
                .With(x => x.TemplateName, "WelcomeTemplate")
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.TemplateName);
        }

        #endregion

        #region Model tests

        [Fact]
        public void Should_Have_Error_When_Model_Is_Null()
        {
            var dto = _fixture.Build<TemplateRequestDTO>()
                .With(x => x.Model, null as object)
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Model)
                  .WithErrorMessage("Template model is required.");
        }

        [Fact]
        public void Should_Not_Have_Error_When_Model_Is_Provided()
        {
            var modelObj = new { Name = "Test" }; // dummy model
            var dto = _fixture.Build<TemplateRequestDTO>()
                .With(x => x.Model, modelObj)
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.Model);
        }

        #endregion
    }
}
