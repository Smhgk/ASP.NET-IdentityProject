using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Options;
using IdentityProject.Settings;

namespace IdentityProject.Services
{
    public class EmailService : IEmailService
    {
        private readonly IOptions<SmtpSetting> _smtpSetting;

        public EmailService(IOptions<SmtpSetting> smtpSetting)
        {
            _smtpSetting = smtpSetting;
        }


        public async Task SendEmailAsync(string from, string to, string subject, string body)
        {
            var message = new MailMessage(from, to, subject, body);

            using (var emailClient = new SmtpClient(_smtpSetting.Value.Host, _smtpSetting.Value.Port))
            {
                emailClient.Credentials = new NetworkCredential(_smtpSetting.Value.Username, _smtpSetting.Value.Password);

                await emailClient.SendMailAsync(message);
            }
        }
    }
}
