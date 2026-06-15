using System.Net;
using System.Net.Mail;

namespace FamiHub.API.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _config;

        public EmailService(ILogger<EmailService> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false)
        {
            try
            {
                var host = _config["EmailSettings:SmtpHost"];
                var port = int.Parse(_config["EmailSettings:SmtpPort"] ?? "587");
                var user = _config["EmailSettings:SmtpUser"];
                var pass = _config["EmailSettings:SmtpPass"];
                var fromName = _config["EmailSettings:FromName"] ?? "FamiHub";

                if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
                {
                    _logger.LogWarning("SMTP settings are not fully configured. Falling back to console log.");
                    _logger.LogInformation("To: {ToEmail}, Subject: {Subject}, Body: {Body}", toEmail, subject, body);
                    return;
                }

                using var client = new SmtpClient(host, port)
                {
                    Credentials = new NetworkCredential(user, pass),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(user, fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };
                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent successfully to {ToEmail}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {ToEmail}", toEmail);
                throw;
            }
        }
    }
}
