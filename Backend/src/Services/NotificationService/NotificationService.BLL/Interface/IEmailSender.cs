namespace NotificationService.BLL.Interface
{
    public interface IEmailSender
    {
        Task SendAsync(string to, string subject, string body);
    }
}
