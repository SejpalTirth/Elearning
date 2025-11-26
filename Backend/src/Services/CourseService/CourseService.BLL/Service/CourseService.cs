using CourseService.BLL.DTOs;
using CourseService.BLL.Interface;
using CourseService.DAL.Models;
using CourseService.DAL.Repo;
using System.Net.Http.Json;

namespace CourseService.BLL.Service
{
    public class Courseservice : ICourseService
    {
        private readonly ICourseRepository _repo;
        private readonly IEnrollmentRepository _enrollRepo;
        private readonly IModuleRepository _moduleRepo;

        private readonly HttpClient _userHttp;
        private readonly HttpClient _progressHttp;

        public Courseservice(
            ICourseRepository repo,
            IEnrollmentRepository enrollRepo,
            IModuleRepository moduleRepo,
            IHttpClientFactory httpClientFactory)
        {
            _repo = repo;
            _enrollRepo = enrollRepo;
            _moduleRepo = moduleRepo;

            _userHttp = httpClientFactory.CreateClient("UserService");
            _progressHttp = httpClientFactory.CreateClient("ProgressService");
        }

        // ------------------- GET ALL -------------------
        public async Task<IEnumerable<CourseResponseDto>> GetAllAsync()
        {
            var courses = await _repo.GetAllAsync();
            var result = new List<CourseResponseDto>();

            foreach (var c in courses)
            {
                var instructor = await FetchInstructorAsync(c.InstructorUserId ?? Guid.Empty);

                var modules = await _moduleRepo.GetByCourseIdAsync(c.Id);

                result.Add(new CourseResponseDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    CategoryId = c.CategoryId,
                    InstructorId = c.InstructorUserId ?? Guid.Empty,
                    InstructorName = instructor?.Name ?? "Unknown Instructor",
                    Modules = modules.Select(m => new ModuleSummaryDto
                    {
                        Id = m.Id,
                        Title = m.Title
                    }).ToList()
                });
            }

            return result;
        }

        // ------------------- GET BY ID -------------------
        public async Task<CourseResponseDto?> GetByIdAsync(int id)
        {
            var c = await _repo.GetByIdAsync(id);
            if (c == null) return null;

            var instructor = await FetchInstructorAsync(c.InstructorUserId ?? Guid.Empty);
            var modules = await _moduleRepo.GetByCourseIdAsync(c.Id);

            return new CourseResponseDto
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                CategoryId = c.CategoryId,
                InstructorId = c.InstructorUserId ?? Guid.Empty,
                InstructorName = instructor?.Name ?? "Unknown Instructor",
                Modules = modules.Select(m => new ModuleSummaryDto
                {
                    Id = m.Id,
                    Title = m.Title,
                    Content = m.Content
                }).ToList()
            };
        }

        // ------------------- CREATE COURSE -------------------
        public async Task<Course> CreateAsync(CourseDto dto)
        {
            var course = new Course
            {
                Title = dto.Title,
                Description = dto.Description,
                CategoryId = dto.CategoryId,
                InstructorUserId = Guid.Parse(dto.InstructorUserId)
            };

            await _repo.AddAsync(course);
            await _repo.SaveChangesAsync();

            if (dto.Modules?.Count > 0)
            {
                foreach (var m in dto.Modules)
                {
                    var moduleEntity = new Module
                    {
                        Title = m.Title,
                        Content = m.Content,
                        CourseId = course.Id
                    };

                    await _moduleRepo.AddAsync(moduleEntity);
                }

                await _moduleRepo.SaveChangesAsync();
            }

            return course;
        }

        // ------------------- UPDATE COURSE -------------------
        public async Task<Course?> UpdateAsync(int id, UpdateCourseDto dto)
        {
            var course = await _repo.GetByIdWithModulesAsync(id);
            if (course == null) return null;

            course.Title = dto.Title;
            course.Description = dto.Description;
            course.CategoryId = dto.CategoryId;

            // Replace modules
            if (dto.Modules != null)
            {
                course.Modules.Clear();

                foreach (var m in dto.Modules)
                {
                    course.Modules.Add(new Module
                    {
                        Title = m.Title,
                        Content = m.Content,
                        CourseId = course.Id
                    });
                }
            }

            await _repo.UpdateAsync(course);
            await _repo.SaveChangesAsync();

            return course;
        }

        // ------------------- ENROLL USER -------------------
        public async Task<bool> EnrollUserAsync(EnrollRequestDto dto)
        {
            bool exists = await _enrollRepo.IsUserEnrolledAsync(dto.UserId, dto.CourseId);
            if (exists) return false;

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

        // ------------------- DELETE COURSE (Updated With Progress Deletion) -------------------
        public async Task<bool> DeleteAsync(int id)
        {
            var course = await _repo.GetByIdWithModulesAsync(id);
            if (course == null) return false;

            course.IsDeleted = true;

            await _repo.UpdateAsync(course);
            await _repo.SaveChangesAsync();

            return true;
        }


        // ------------------- FETCH INSTRUCTOR FROM USER SERVICE -------------------
        private async Task<InstructorDto> FetchInstructorAsync(Guid id)
        {
            try
            {
                var user = await _userHttp.GetFromJsonAsync<UserAuthDto>($"api/users/{id}");

                return user == null
                    ? new InstructorDto { Id = id, Name = "Unknown Instructor" }
                    : new InstructorDto
                    {
                        Id = user.Id,
                        Name = string.IsNullOrWhiteSpace(user.Name) ? user.Email : user.Name
                    };
            }
            catch
            {
                return new InstructorDto { Id = id, Name = "Unknown Instructor" };
            }
        }

        public async Task<IEnumerable<Course>> GetUserEnrolledCoursesAsync(string userId)
        {
            var enrollments = await _enrollRepo.GetByUserIdAsync(userId);
            var ids = enrollments.Select(e => e.CourseId).ToList();

            return ids.Count == 0 ? new List<Course>() : await _repo.GetByIdsAsync(ids);
        }

        public async Task<IEnumerable<Course>> GetCoursesByInstructorAsync(Guid instructorId)
        {
            return await _repo.GetByInstructorIdAsync(instructorId);
        }
    }
}
