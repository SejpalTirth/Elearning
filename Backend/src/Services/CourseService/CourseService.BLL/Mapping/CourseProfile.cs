using AutoMapper;
using CourseService.BLL.DTOs;
using CourseService.DAL.Models;

public class CourseProfile : Profile
{
    public CourseProfile()
    {
        // -----------------------------
        // ENTITY → RESPONSE DTO
        // -----------------------------
        CreateMap<Course, CourseResponseDto>()
            .ForMember(dest => dest.InstructorId,
                opt => opt.MapFrom(src => src.InstructorUserId ?? Guid.Empty))
            .ForMember(dest => dest.InstructorName,
                opt => opt.Ignore()) // Filled from UserService
            .ForMember(dest => dest.Modules,
                opt => opt.MapFrom(src => src.Modules));

        CreateMap<Module, ModuleSummaryDto>();

        // -----------------------------
        // CREATE COURSE
        // -----------------------------
        CreateMap<CourseDto, Course>()
            .ForMember(dest => dest.InstructorUserId,
                opt => opt.MapFrom(src => Guid.Parse(src.InstructorUserId)))
            .ForMember(dest => dest.Modules,
                opt => opt.MapFrom(src => src.Modules))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(_ => false))
            .ForMember(dest => dest.IsDraft, opt => opt.MapFrom(_ => true));

        CreateMap<ModuleDto, Module>();

        // -----------------------------
        // UPDATE COURSE (safe overwrite)
        // -----------------------------
        CreateMap<UpdateCourseDto, Course>()
            .ForMember(dest => dest.Modules, opt => opt.Ignore());

        CreateMap<UpdateModuleDto, Module>();
    }
}
