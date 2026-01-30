using Microsoft.AspNetCore.Http;
using NotificationService.BLL.DTOs;
using NotificationService.BLL.Models;
using ProgresService.BLL.Interface;
using ProgresService.BLL.Models;
using ProgresService.DAL.Repo;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ProgresService.BLL.Service
{
    public class ProgressServiceImpl : IProgressService
    {
        private readonly IProgressRepository _repo;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProgressServiceImpl(
            IProgressRepository repo,
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _repo = repo;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<ProgresDto>> GetUserProgressAsync(Guid userId)
        {
            var progress = await _repo.GetUserProgressAsync(userId);

            return progress.Select(p => new ProgresDto
            {
                CourseId = p.CourseId,
                ModuleId = p.ModuleId,
                ProgresPercent = p.ProgressPercent,
                IsCompleted = p.IsCompleted
            }).ToList();
        }

        public async Task MarkModuleCompletedAsync(
            Guid userId,
            int courseId,
            int moduleId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("Invalid userId");

            if (courseId <= 0 || moduleId <= 0)
                throw new ArgumentException("Invalid courseId or moduleId");

            // 1️⃣ Persist progress (primary responsibility)
            await _repo.MarkModuleCompleteAsync(userId, courseId, moduleId);

            // 2️⃣ Notifications (secondary – never break progress)
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var authHeader = httpContext?.Request.Headers["Authorization"].ToString();

                var userName = GetUserName();
                var userEmail = GetUserEmail();

                var (courseName, moduleName, totalModules) =
                    await GetCourseAndModuleInfoAsync(courseId, moduleId, authHeader);

                var notificationClient =
                    _httpClientFactory.CreateClient("NotificationService");

                if (!string.IsNullOrWhiteSpace(authHeader))
                {
                    notificationClient.DefaultRequestHeaders.Authorization =
                        AuthenticationHeaderValue.Parse(authHeader);
                }

                // ---------------- MODULE COMPLETED ----------------
                await notificationClient.PostAsJsonAsync(
                    "/api/notification/trigger",
                    new TriggerNotificationDto
                    {
                        UserId = userId,
                        Email = userEmail,
                        Type = NotificationType.ModuleCompleted,
                        Data = new Dictionary<string, string>
                        {
                            { "UserName", userName },
                            { "CourseName", courseName },
                            { "ModuleName", moduleName }
                        }
                    });

                // ---------------- COURSE COMPLETED ----------------
                bool courseCompleted =
                    await _repo.IsCourseFullyCompletedAsync(
                        userId,
                        courseId,
                        totalModules
                    );

                if (courseCompleted)
                {
                    await notificationClient.PostAsJsonAsync(
                        "/api/notification/trigger",
                        new TriggerNotificationDto
                        {
                            UserId = userId,
                            Email = userEmail,
                            Type = NotificationType.CourseCompleted,
                            Data = new Dictionary<string, string>
                            {
                                { "UserName", userName },
                                { "CourseName", courseName }
                            }
                        });
                }
            }
            catch
            {
                // ❗ Notifications must never break progress tracking
            }
        }

        // ---------------- Helpers ----------------

        private string GetUserName()
        {
            return _httpContextAccessor.HttpContext?.User?
                .Claims
                .FirstOrDefault(c => c.Type == "name")
                ?.Value
                ?? "Learner";
        }

        private string GetUserEmail()
        {
            return _httpContextAccessor.HttpContext?.User?
                .Claims
                .FirstOrDefault(c =>
                    c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")
                ?.Value
                ?? string.Empty;
        }

        private async Task<(string courseName, string moduleName, int totalModules)>
        GetCourseAndModuleInfoAsync(int courseId, int moduleId, string authHeader)
        {
            var courseClient = _httpClientFactory.CreateClient("CourseService");

            if (!string.IsNullOrWhiteSpace(authHeader))
            {
                courseClient.DefaultRequestHeaders.Authorization =
                    System.Net.Http.Headers.AuthenticationHeaderValue.Parse(authHeader);
            }

            //  Course
            var courseResponse = await courseClient.PostAsJsonAsync(
                "/api/courses/by-id",
                new { courseId }
            );

            var course = await courseResponse.Content
                .ReadFromJsonAsync<CourseDto>();

            // 🔹 Modules
            var modulesResponse = await courseClient.PostAsJsonAsync(
                "/api/modules/by-course",
                new { courseId }
            );

            var modules = await modulesResponse.Content
                .ReadFromJsonAsync<List<ModuleDto>>() ?? new();

            var moduleName = modules
                .FirstOrDefault(m => m.Id == moduleId)
                ?.Title ?? "your module";

            return (
                course?.Title ?? "your course",
                moduleName,
                modules.Count
            );
        }


        // ---------------- Local DTOs ----------------

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
