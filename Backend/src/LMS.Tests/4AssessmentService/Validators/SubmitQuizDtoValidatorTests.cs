using AutoFixture;
using DTOs._4AssessmentService;
using FluentValidation.TestHelper;
using AssessmentService.BLL.Validators;
using System;
using System.Collections.Generic;
using Xunit;

namespace LMS.Tests._4AssessmentService.Validators
{
    public class SubmitQuizDtoValidatorTests
    {
        private readonly IFixture _fixture;
        private readonly SubmitQuizDtoValidator _validator;

        public SubmitQuizDtoValidatorTests()
        {
            _fixture = new Fixture();
            _validator = new SubmitQuizDtoValidator();
        }

        #region UserId tests

        [Fact]
        public void Should_Have_Error_When_UserId_Is_Empty()
        {
            var dto = _fixture.Build<SubmitQuizDto>()
                .With(x => x.UserId, Guid.Empty)
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.UserId)
                  .WithErrorMessage("UserId is required.");
        }

        [Fact]
        public void Should_Not_Have_Error_When_UserId_Is_Valid()
        {
            var dto = _fixture.Build<SubmitQuizDto>()
                .With(x => x.UserId, Guid.NewGuid())
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.UserId);
        }

        #endregion

        #region QuizId tests

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Should_Have_Error_When_QuizId_Is_Not_Positive(int invalidQuizId)
        {
            var dto = _fixture.Build<SubmitQuizDto>()
                .With(x => x.QuizId, invalidQuizId)
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.QuizId)
                  .WithErrorMessage("QuizId must be greater than zero.");
        }

        [Fact]
        public void Should_Not_Have_Error_When_QuizId_Is_Positive()
        {
            var dto = _fixture.Build<SubmitQuizDto>()
                .With(x => x.QuizId, 1)
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.QuizId);
        }

        #endregion

        #region Answers tests

        [Fact]
        public void Should_Have_Error_When_Answers_Is_Null()
        {
            var dto = _fixture.Build<SubmitQuizDto>()
                .With(x => x.Answers, (List<SubmitAnswerDto>)null)
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Answers)
                  .WithErrorMessage("At least one answer must be submitted.");
        }

        [Fact]
        public void Should_Have_Error_When_Answers_Is_Empty()
        {
            var dto = _fixture.Build<SubmitQuizDto>()
                .With(x => x.Answers, new List<SubmitAnswerDto>())
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Answers)
                  .WithErrorMessage("At least one answer must be submitted.");
        }

        [Fact]
        public void Should_Not_Have_Error_When_Answers_Has_Items()
        {
            var answer = _fixture.Build<SubmitAnswerDto>()
                .With(x => x.QuestionId, 1)
                .With(x => x.SelectedAnswerId, 0)
                .Create();

            var dto = _fixture.Build<SubmitQuizDto>()
                .With(x => x.Answers, new List<SubmitAnswerDto> { answer })
                .With(x => x.UserId, Guid.NewGuid()) // valid GUID
                .Create();

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.Answers);
        }

        #endregion
    }
}
