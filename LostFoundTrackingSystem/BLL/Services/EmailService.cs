using BLL.IServices;
using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var smtpSettings = _configuration.GetSection("SmtpSettings");
            var host = smtpSettings["Host"];
            var port = int.Parse(smtpSettings["Port"]);
            var username = smtpSettings["Username"];
            var password = smtpSettings["Password"];
            var senderEmail = smtpSettings["SenderEmail"];
            var senderName = smtpSettings["SenderName"];

            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(senderEmail))
            {
                Console.WriteLine("--> SMTP Email service is not configured correctly in appsettings.json. Skipping email send.");
                return;
            }

            try
            {
                using (var client = new SmtpClient(host, port))
                {
                    client.EnableSsl = true;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(username, password);

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(senderEmail, senderName),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true,
                    };
                    mailMessage.To.Add(to);

                    Console.WriteLine($"--> Attempting to send email to {to} via SMTP.");
                    await client.SendMailAsync(mailMessage);
                    Console.WriteLine($"--> Email sent successfully via SMTP.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> An exception occurred while sending email via SMTP: {ex}");
            }
        }
    }
}
