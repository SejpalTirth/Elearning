using System;
using AutoFixture;
using DTOs._6NotificationService;
using FluentValidation.TestHelper;
using NotificationService.BLL.Validators;
using Xunit;

namespace LMS.Tests._6NotificationService.Validators
{
    public class EmailRequestValidatorTests
    {
        private readonly IFixture _fixture;
        private readonly EmailRequestValidator _validator;

        public EmailRequestValidatorTests()
        {
            _fixture = new Fixture();
            _validator = new EmailRequestValidator();
        }

        #region UserId tests

        [Fact]
        public void Should_Have_Error_When_UserId_Is_Empty()
        {
            var dto = _fixture.Build<EmailRequestDTO>()
                .With(x => x.UserId, Guid.Empty)
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.UserId)
                  .WithErrorMessage("UserId is required.");
        }

        [Fact]
        public void Should_Not_Have_Error_When_UserId_Is_Provided()
        {
            var dto = _fixture.Build<EmailRequestDTO>()
                .With(x => x.UserId, Guid.NewGuid())
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.UserId);
        }

        #endregion

        #region Subject tests

        [Fact]
        public void Should_Have_Error_When_Subject_Is_Empty()
        {
            var dto = _fixture.Build<EmailRequestDTO>()
                .With(x => x.Subject, string.Empty)
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Subject)
                  .WithErrorMessage("Email subject is required.");
        }

        [Fact]
        public void Should_Not_Have_Error_When_Subject_Is_Provided()
        {
            var dto = _fixture.Build<EmailRequestDTO>()
                .With(x => x.Subject, "Test Subject")
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.Subject);
        }

        #endregion

        #region Body tests

        [Fact]
        public void Should_Have_Error_When_Body_Is_Empty()
        {
            var dto = _fixture.Build<EmailRequestDTO>()
                .With(x => x.Body, string.Empty)
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Body)
                  .WithErrorMessage("Email body is required.");
        }

        [Fact]
        public void Should_Not_Have_Error_When_Body_Is_Provided()
        {
            var dto = _fixture.Build<EmailRequestDTO>()
                .With(x => x.Body, "This is a test email body.")
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.Body);
        }

        #endregion
    }
}
