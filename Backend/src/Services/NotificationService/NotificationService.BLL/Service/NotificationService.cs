using NotificationService.BLL.Interface;
using NotificationService.BLL.Models;
using NotificationService.DAL.Repo;
using NotificationService.DAL.Models;

namespace NotificationService.BLL.Service
{
    public class NotificationServiceImpl : INotificationService
    {
        private readonly INotificationRepository _repo;
        private readonly IEmailSender _email;

        public NotificationServiceImpl(INotificationRepository repo, IEmailSender email)
        {
            _repo = repo;
            _email = email;
        }

        public async Task SendEmailAsync(Guid userId, string subject, string body)
        {
            await _email.SendAsync("user-email-placeholder", subject, body);

            await _repo.SaveNotificationAsync(new Notification
            {
                UserId = userId,
                Title = subject,
                Body = body,
                IsRead = false,
                SentAt = DateTime.Now
            });
        }

        public async Task SendEmailByTemplateAsync(Guid userId, string templateName, object model)
        {
            var template = await _repo.GetTemplateByNameAsync(templateName);
            if (template == null) throw new Exception("Template not found");

            var body = EmailTemplateRenderer.Render(template.BodyTemplate, model);

            await SendEmailAsync(userId, template.Subject, body);
        }

        public async Task<List<NotificationDto>> GetUserNotificationsAsync(Guid userId)
        {
            var notifications = await _repo.GetUserNotificationsAsync(userId);

            return notifications.Select(n => new NotificationDto
            {
                Title = n.Title,
                Body = n.Body,
                SentAt = n.SentAt
            }).ToList();
        }

        public async Task SendEmailDirectAsync(string email, string subject, string body)
        {
            await _email.SendAsync(email, subject, body);

            await _repo.SaveNotificationAsync(new Notification
            {
                UserId = null,
                Title = subject,
                Body = body,
                IsRead = false,
                SentAt = DateTime.Now
            });
        }

    }
}
