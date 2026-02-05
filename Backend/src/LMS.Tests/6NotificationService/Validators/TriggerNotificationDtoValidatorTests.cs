using System;
using System.Collections.Generic;
using AutoFixture;
using DTOs._6NotificationService;
using FluentValidation.TestHelper;
using NotificationService.BLL.Validators;
using Xunit;

namespace LMS.Tests._6NotificationService.Validators
{
    public class TriggerNotificationDtoValidatorTests
    {
        private readonly Fixture _fixture;
        private readonly TriggerNotificationDtoValidator _validator;

        public TriggerNotificationDtoValidatorTests()
        {
            _fixture = new Fixture();
            _validator = new TriggerNotificationDtoValidator();
        }

        [Fact]
        public void Should_Have_Error_When_UserId_Is_Empty()
        {
            var dto = _fixture.Build<TriggerNotificationDto>()
                .With(x => x.UserId, Guid.Empty)
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.UserId)
                  .WithErrorMessage("UserId is required.");
        }

        [Fact]
        public void Should_Not_Have_Error_When_UserId_Is_Provided()
        {
            var dto = _fixture.Build<TriggerNotificationDto>()
                .With(x => x.UserId, Guid.NewGuid())
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.UserId);
        }

        [Fact]
        public void Should_Have_Error_When_Email_Is_NullOrEmpty()
        {
            var dto = _fixture.Build<TriggerNotificationDto>()
                .With(x => x.Email, string.Empty)
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Email)
                  .WithErrorMessage("Email is required.");
        }

        [Fact]
        public void Should_Have_Error_When_Data_Is_Null()
        {
            var dto = _fixture.Build<TriggerNotificationDto>()
                .With(x => x.Data, null as Dictionary<string, string>)
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Data)
                  .WithErrorMessage("Notification data is required.");
        }

        [Fact]
        public void Should_Have_Error_When_Data_Is_Empty()
        {
            var dto = _fixture.Build<TriggerNotificationDto>()
                .With(x => x.Data, new Dictionary<string, string>())
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Data)
                  .WithErrorMessage("Notification data is required.");
        }

        [Fact]
        public void Should_Not_Have_Error_When_Data_Is_Provided()
        {
            var dto = _fixture.Build<TriggerNotificationDto>()
                .With(x => x.Data, new Dictionary<string, string> { { "key", "value" } })
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.Data);
        }

        [Fact]
        public void Should_Have_Error_When_Type_Is_Invalid()
        {
            var dto = _fixture.Build<TriggerNotificationDto>()
                .With(x => x.Type, (NotificationType)999)
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Type)
                  .WithErrorMessage("Invalid notification type.");
        }

        [Fact]
        public void Should_Not_Have_Error_When_All_Valid()
        {
            var dto = _fixture.Build<TriggerNotificationDto>()
                .With(x => x.UserId, Guid.NewGuid())
                .With(x => x.Email, "user@example.com")
                .With(x => x.Type, NotificationType.Enrollment)
                .With(x => x.Data, new Dictionary<string, string> { { "key", "value" } })
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
