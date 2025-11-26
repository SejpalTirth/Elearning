using System.Collections.Generic;
using System;

namespace ProgressService.BLL.DTOs
{
    public enum NotificationType
    {
        Enrollment,
        ModuleCompleted,
        CourseCompleted
    }

    public class TriggerNotificationDto
    {
        public Guid UserId { get; set; }
        public NotificationType Type { get; set; }
        public string Email { get; set; }
        public Dictionary<string, string> Data { get; set; } = new();
    }
}
