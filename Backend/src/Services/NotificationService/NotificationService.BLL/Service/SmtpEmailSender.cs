using Microsoft.Extensions.Configuration;
using NotificationService.BLL.Interface;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace NotificationService.BLL.Service
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly IConfiguration _config;

        public SmtpEmailSender(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendAsync(string to, string subject, string body)
        {
            var smtpHost = _config["Smtp:Host"];
            var smtpPort = int.Parse(_config["Smtp:Port"]);
            var smtpUser = _config["Smtp:Username"];
            var smtpPass = _config["Smtp:Password"];

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            // Encode subject for emoji support
            string encodedSubject = $"=?utf-8?B?{Convert.ToBase64String(Encoding.UTF8.GetBytes(subject))}?=";

            var mail = new MailMessage
            {
                From = new MailAddress(smtpUser, "E-Learning Platform", Encoding.UTF8),
                Subject = encodedSubject,
                SubjectEncoding = Encoding.UTF8,
                Body = body,
                BodyEncoding = Encoding.UTF8,
                HeadersEncoding = Encoding.UTF8,
                IsBodyHtml = true
            };

            mail.To.Add(new MailAddress(to, to, Encoding.UTF8));

            // Required for Gmail to accept emojis
            mail.Headers.Add("Content-Transfer-Encoding", "base64");

            await client.SendMailAsync(mail);
        }
    }
}
