using AutoMapper;
using CourseService.BLL.DTOs;
using CourseService.BLL.Interface;
using CourseService.DAL.Models;
using CourseService.DAL.Repo;
using System.Net.Http.Json;

namespace CourseService.BLL.Service
{
    public class CourseServiceimpl : ICourseService
    {
        private readonly ICourseRepository _repo;
        private readonly IEnrollmentRepository _enrollRepo;
        private readonly IModuleRepository _moduleRepo;
        private readonly IHttpClientFactory _httpFactory;
        private readonly IMapper _mapper;

        public CourseServiceimpl(
            ICourseRepository repo,
            IEnrollmentRepository enrollRepo,
            IModuleRepository moduleRepo,
            IHttpClientFactory httpFactory,
            IMapper mapper)
        {
            _repo = repo;
            _enrollRepo = enrollRepo;
            _moduleRepo = moduleRepo;
            _httpFactory = httpFactory;
            _mapper = mapper;
        }

        // --------------------------------------------------------------------
        // GET ALL COURSES
        // --------------------------------------------------------------------
        public async Task<IEnumerable<CourseResponseDto>> GetAllAsync()
        {
            var courses = await _repo.GetAllAsync();
            var result = new List<CourseResponseDto>();

            foreach (var course in courses)
            {
                var instructor = await FetchInstructorAsync(course.InstructorUserId ?? Guid.Empty);

                var dto = _mapper.Map<CourseResponseDto>(course);
                dto.InstructorName = instructor?.Name ?? "Unknown Instructor";

                result.Add(dto);
            }

            return result;
        }


        // --------------------------------------------------------------------
        // GET COURSE BY ID
        // --------------------------------------------------------------------
        public async Task<CourseResponseDto?> GetByIdAsync(int id)
        {
            var course = await _repo.GetByIdWithModulesAsync(id);
            if (course == null) return null;

            var instructor = await FetchInstructorAsync(course.InstructorUserId ?? Guid.Empty);

            var dto = _mapper.Map<CourseResponseDto>(course);
            dto.InstructorName = instructor?.Name ?? "Unknown Instructor";

            return dto;
        }

        // --------------------------------------------------------------------
        // CREATE COURSE (Mark IsDeleted=true so it's private until quizzes completed)
        // --------------------------------------------------------------------
        public async Task<Course> CreateAsync(CourseDto dto)
        {
            var course = _mapper.Map<Course>(dto);

            await _repo.AddAsync(course);
            await _repo.SaveChangesAsync();

            return course;
        }

        // --------------------------------------------------------------------
        // UPDATE COURSE
        // --------------------------------------------------------------------
        public async Task<Course?> UpdateAsync(int id, UpdateCourseDto dto)
        {
            var course = await _repo.GetByIdWithModulesAsync(id);
            if (course == null) return null;

            _mapper.Map(dto, course);

            var existingModules = course.Modules.ToList();

            foreach (var m in dto.Modules)
            {
                if (m.Id > 0)
                {
                    var existing = existingModules.FirstOrDefault(x => x.Id == m.Id);
                    if (existing != null)
                        _mapper.Map(m, existing);
                }
                else
                {
                    course.Modules.Add(_mapper.Map<Module>(m));
                }
            }

            var dtoIds = dto.Modules.Where(x => x.Id > 0).Select(x => x.Id).ToList();
            var removed = existingModules.Where(x => !dtoIds.Contains(x.Id));

            foreach (var rm in removed)
                _repo.RemoveModule(rm);

            await _repo.SaveChangesAsync();

            // quiz logic unchanged
            await HandlePublishStateAsync(course);

            return course;
        }

        private async Task HandlePublishStateAsync(Course course)
        {
            var client = _httpFactory.CreateClient("AssessmentService");
            var quizResp = await client.GetAsync($"api/assessment/course-status/{course.Id}");

            bool allQuizzesCreated = false;

            if (quizResp.IsSuccessStatusCode)
            {
                var data = await quizResp.Content.ReadFromJsonAsync<CourseQuizStatusDto>();
                allQuizzesCreated = data?.AllQuizzesCreated ?? false;
            }

            course.IsDraft = !allQuizzesCreated;
            course.IsDeleted = false;

            await _repo.SaveChangesAsync();
        }


        // --------------------------------------------------------------------
        // ENROLL USER
        // --------------------------------------------------------------------
        public async Task<bool> EnrollUserAsync(EnrollRequestDto dto)
        {
            if (await _enrollRepo.IsUserEnrolledAsync(dto.UserId, dto.CourseId))
                return false;

            var enrollment = new Enrollment
            {
                CourseId = dto.CourseId,
                UserId = dto.UserId,
                EnrolledAt = DateTime.UtcNow
            };

            await _enrollRepo.AddAsync(enrollment);
            await _enrollRepo.SaveChangesAsync();
            return true;
        }

        // --------------------------------------------------------------------
        // UNFINISHED COURSE (Instructor pending task)
        // --------------------------------------------------------------------
        public async Task<IEnumerable<Course>> GetAllUnfinishedCoursesAsync(Guid instructorId)
        {
            var courses = await _repo.GetAllAsync();

            return courses
                .Where(c =>
                    c.InstructorUserId == instructorId &&
                    c.IsDraft == true &&
                    c.IsDeleted == false
                )
                .ToList();
        }

        // --------------------------------------------------------------------
        // CONTINUE COURSE (Just allow editing; DO NOT PUBLISH)
        // --------------------------------------------------------------------
        public async Task<bool> ContinueUnfinishedCourseAsync(int courseId)
        {
            // Instructor just wants to edit — do NOT publish here.
            var course = await _repo.GetByIdAsync(courseId);
            return course != null;
        }

        // --------------------------------------------------------------------
        // AUTO-PUBLISH CHECK (Only when all quizzes exist)
        // --------------------------------------------------------------------
        public async Task<bool> PublishCourseIfReadyAsync(int courseId)
        {
            var course = await _repo.GetByIdAllowDeletedAsync(courseId); // FIXED
            if (course == null)
                return false;

            var modules = await _moduleRepo.GetByCourseIdAsync(courseId);
            if (!modules.Any())
                return false;

            var client = _httpFactory.CreateClient("AssessmentService");
            var missing = await client.GetFromJsonAsync<List<int>>(
                $"/api/Assessment/unquizzed-modules/{courseId}"
            );

            if (missing == null || missing.Any())
                return false;

            course.IsDeleted = false;
            course.IsDraft = false;

            await _repo.UpdateAsync(course);
            await _repo.SaveChangesAsync();

            return true;
        }

        // --------------------------------------------------------------------
        // USER COURSES
        // --------------------------------------------------------------------
        public async Task<IEnumerable<Course>> GetUserEnrolledCoursesAsync(string userId)
        {
            var enrollments = await _enrollRepo.GetByUserIdAsync(userId);
            var ids = enrollments.Select(e => e.CourseId).ToList();

            return ids.Any() ? await _repo.GetByIdsAsync(ids) : new List<Course>();
        }

        // --------------------------------------------------------------------
        // FETCH INSTRUCTOR FROM USER SERVICE
        // --------------------------------------------------------------------
        private async Task<InstructorDto> FetchInstructorAsync(Guid id)
        {
            try
            {
                var client = _httpFactory.CreateClient("UserService");
                var user = await client.GetFromJsonAsync<UserAuthDto>($"api/users/{id}");

                return new InstructorDto
                {
                    Id = id,
                    Name = user?.Name ?? user?.Email ?? "Unknown Instructor"
                };
            }
            catch
            {
                return new InstructorDto
                {
                    Id = id,
                    Name = "Unknown Instructor"
                };
            }
        }

        // --------------------------------------------------------------------
        // GET COURSES BY INSTRUCTOR
        // --------------------------------------------------------------------
        public async Task<IEnumerable<Course>> GetCoursesByInstructorAsync(Guid instructorId)
        {
            return await _repo.GetByInstructorIdAsync(instructorId);
        }

        // --------------------------------------------------------------------
        // DELETE (Soft delete)
        // --------------------------------------------------------------------
        public async Task<bool> DeleteAsync(int id)
        {
            var course = await _repo.GetByIdWithModulesAsync(id);
            if (course == null) return false;

            course.IsDeleted = true;
            course.IsDraft = false;

            await _repo.UpdateAsync(course);
            await _repo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestoreAsync(int id)
        {
            var course = await _repo.GetByIdAllowDeletedAsync(id);
            if (course == null)
                return false;

            course.IsDeleted = false;
            
            await _repo.UpdateAsync(course);
            await _repo.SaveChangesAsync();
            return true;
        }

        // Helper DTO
        private class InstructorDto
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }
    }
}
