using System.Net;
using System.Net.Mail;

namespace GrowMate.Services.EmailRegister
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config) => _config = config;

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            using var smtpClient = new SmtpClient(_config["Email:Smtp:Host"])
            {
                Port = int.Parse(_config["Email:Smtp:Port"]!),
                Credentials = new NetworkCredential(
                    _config["Email:Smtp:Username"],
                    _config["Email:Smtp:Password"]
                ),
                EnableSsl = true,
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_config["Email:Smtp:From"]!),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(to);
            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
