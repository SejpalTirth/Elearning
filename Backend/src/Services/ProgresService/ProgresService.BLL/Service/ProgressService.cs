using NotificationService.BLL.DTOs;
using NotificationService.BLL.Models;
using ProgresService.DAL.Repo;
using ProgressService.BLL.Interface;
using ProgressService.BLL.Models;
using System.Net.Http.Json;

namespace ProgressService.BLL.Service
{
    public class ProgressServiceImpl : IProgressService
    {
        private readonly IProgressRepository _repo;
        private readonly IHttpClientFactory _httpClientFactory;

        public ProgressServiceImpl(IProgressRepository repo, IHttpClientFactory httpClientFactory)
        {
            _repo = repo;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<List<ProgressDto>> GetUserProgressAsync(Guid userId)
        {
            var progress = await _repo.GetUserProgressAsync(userId);

            return progress.Select(p => new ProgressDto
            {
                CourseId = p.CourseId,
                ModuleId = p.ModuleId,
                ProgressPercent = p.ProgressPercent,
                IsCompleted = p.IsCompleted
            }).ToList();
        }

        public async Task MarkModuleCompletedAsync(Guid userId, int courseId, int moduleId)
        {
            // Update progress in DB
            await _repo.MarkModuleCompleteAsync(userId, courseId, moduleId);

            // Fetch User Info from User Service
            var userClient = _httpClientFactory.CreateClient("UserService");
            var user = await userClient.GetFromJsonAsync<UserDto>($"api/users/{userId}");

            if (user == null)
                throw new Exception("Unable to send notification — User not found.");

            // Fetch module and course details
            var courseClient = _httpClientFactory.CreateClient("CourseService");

            // Fetch all modules for this course
            var moduleList = await courseClient.GetFromJsonAsync<List<ModuleDto>>($"api/courses/{courseId}/modules")
                            ?? new List<ModuleDto>();

            // Resolve the specific module name
            var module = moduleList.FirstOrDefault(m => m.Id == moduleId);
            string moduleName = !string.IsNullOrWhiteSpace(module?.Title) ? module.Title : $"Module {moduleId}";

            // Fetch course title
            var course = await courseClient.GetFromJsonAsync<CourseDto>($"api/courses/{courseId}");
            string courseName = course?.Title ?? "Course";

            // Determine if ALL modules are completed
            int totalModules = moduleList.Count;
            int completedModules = await _repo.GetCompletedModuleCountAsync(userId, courseId);

            bool courseCompleted = totalModules > 0 && completedModules == totalModules;

            // Send notification via NotificationService
            // Send MODULE COMPLETED notification ALWAYS
            var notifyClient = _httpClientFactory.CreateClient("NotificationService");

            var moduleCompletedNotification = new TriggerNotificationDto
            {
                UserId = userId,
                Email = user.Email,
                Type = NotificationType.ModuleCompleted,
                Data = new Dictionary<string, string>
    {
        { "UserName", user.Name ?? "User" },
        { "CourseName", courseName },
        { "ModuleName", moduleName }
    }
            };

            await notifyClient.PostAsJsonAsync("/api/notification/trigger", moduleCompletedNotification);

            // If last module, ALSO send COURSE COMPLETED notification
            if (courseCompleted)
            {
                var courseCompletedNotification = new TriggerNotificationDto
                {
                    UserId = userId,
                    Email = user.Email,
                    Type = NotificationType.CourseCompleted,
                    Data = new Dictionary<string, string>
        {
            { "UserName", user.Name ?? "User" },
            { "CourseName", courseName }
        }
                };

                await notifyClient.PostAsJsonAsync("/api/notification/trigger", courseCompletedNotification);
            }

        }


        private class UserDto
        {
            public string Email { get; set; }
            public string Name { get; set; }
        }

        private class ModuleDto
        {
            public int Id { get; set; }
            public string Title { get; set; }
        }

        private class CourseDto
        {
            public string Title { get; set; }
        }
    }
}
