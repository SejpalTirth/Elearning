using AutoMapper;
using DTOs._4AssessmentService;
using AssessmentService.DAL.Models;

public class AssessmentProfile : Profile
{
    public AssessmentProfile()
    {
        // -------------------------
        // QUIZ → DTOs
        // -------------------------

        CreateMap<Quiz, QuizSummaryDto>();

        CreateMap<Quiz, QuizDetailDto>()
            .ForMember(d => d.TotalMarks,
                o => o.MapFrom(s => s.TotalMarks ?? s.Questions.Sum(q => q.Marks)));

        CreateMap<Quiz, QuizForModuleDto>()
            .ForMember(d => d.QuizId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.TotalMarks,
                o => o.MapFrom(s => s.Questions.Sum(q => q.Marks)))
            .ForMember(d => d.AlreadyPassed, o => o.Ignore())
            .ForMember(d => d.ModuleId, o => o.Ignore());

        // -------------------------
        // QUESTION → DTOs
        // -------------------------

        CreateMap<Question, QuestionDetailDto>()
            .ForMember(d => d.Text, o => o.MapFrom(s => s.QuestionText));

        // -------------------------
        // ANSWER → DTOs
        // -------------------------

        CreateMap<Answer, AnswerOptionDto>()
            .ForMember(d => d.Text, o => o.MapFrom(s => s.AnswerText));

        // -------------------------
        // CREATE DTOs → ENTITIES
        // -------------------------

        CreateMap<CreateQuizDto, Quiz>()
            .ForMember(d => d.TotalMarks, o => o.MapFrom(_ => 0));

        CreateMap<CreateQuestionDto, Question>()
            .ForMember(d => d.QuestionText, o => o.MapFrom(s => s.Question))
            .ForMember(d => d.QuestionType, o => o.MapFrom(_ => "MCQ"))
            .ForMember(d => d.Answers, o => o.Ignore());
    }
}
