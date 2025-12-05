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

        public CourseServiceimpl(
            ICourseRepository repo,
            IEnrollmentRepository enrollRepo,
            IModuleRepository moduleRepo,
            IHttpClientFactory httpFactory)
        {
            _repo = repo;
            _enrollRepo = enrollRepo;
            _moduleRepo = moduleRepo;
            _httpFactory = httpFactory;
        }

        // --------------------------------------------------------------------
        // GET ALL COURSES
        // --------------------------------------------------------------------
        public async Task<IEnumerable<CourseResponseDto>> GetAllAsync()
        {
            var courses = await _repo.GetAllAsync();
            var output = new List<CourseResponseDto>();

            foreach (var c in courses)
            {
                var instructor = await FetchInstructorAsync(c.InstructorUserId ?? Guid.Empty);
                var modules = await _moduleRepo.GetByCourseIdAsync(c.Id);

                output.Add(new CourseResponseDto
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
                });
            }

            return output;
        }

        // --------------------------------------------------------------------
        // GET COURSE BY ID
        // --------------------------------------------------------------------
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

        // --------------------------------------------------------------------
        // CREATE COURSE (Mark IsDeleted=true so it's private until quizzes completed)
        // --------------------------------------------------------------------
        public async Task<Course> CreateAsync(CourseDto dto)
        {
            var course = new Course
            {
                Title = dto.Title,
                Description = dto.Description,
                CategoryId = dto.CategoryId,
                InstructorUserId = Guid.Parse(dto.InstructorUserId),
                IsDeleted = true // UNPUBLISHED UNTIL QUIZZES DONE
            };

            await _repo.AddAsync(course);
            await _repo.SaveChangesAsync();

            // Create Modules
            if (dto.Modules != null)
            {
                foreach (var m in dto.Modules)
                {
                    await _moduleRepo.AddAsync(new Module
                    {
                        Title = m.Title,
                        Content = m.Content,
                        CourseId = course.Id
                    });
                }

                await _moduleRepo.SaveChangesAsync();
            }

            return course;
        }

        // --------------------------------------------------------------------
        // UPDATE COURSE
        // --------------------------------------------------------------------
        public async Task<Course?> UpdateAsync(int id, UpdateCourseDto dto)
        {
            var course = await _repo.GetByIdWithModulesAsync(id);
            if (course == null) return null;

            course.Title = dto.Title;
            course.Description = dto.Description;
            course.CategoryId = dto.CategoryId;

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
        public async Task<object?> GetUnfinishedCourseAsync(Guid instructorId)
        {
            // Only courses with IsDeleted = true (i.e., unpublished)
            var course = await _repo.GetLatestUnfinishedCourseAsync(instructorId);
            if (course == null) return null;

            return new
            {
                id = course.Id,
                title = course.Title,
                description = course.Description,
                categoryId = course.CategoryId
            };
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

            // ✓ All quizzes complete → publish it
            course.IsDeleted = false;

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
