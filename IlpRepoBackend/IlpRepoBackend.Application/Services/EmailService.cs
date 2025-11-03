using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly IEmailConfigurationRepository _emailConfigRepository;
        private readonly IEmailLogRepository _emailLogRepository;
        private readonly IConfiguration _configuration;

        public EmailService(
            IEmailConfigurationRepository emailConfigRepository,
            IEmailLogRepository emailLogRepository,
            IConfiguration configuration)
        {
            _emailConfigRepository = emailConfigRepository;
            _emailLogRepository = emailLogRepository;
            _configuration = configuration;
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var smtpHost = _configuration["Email:SmtpHost"];
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var smtpUsername = _configuration["Email:SmtpUsername"];
                var smtpPassword = _configuration["Email:SmtpPassword"];
                var fromEmail = _configuration["Email:FromEmail"];
                var fromName = _configuration["Email:FromName"];

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(to);

                await client.SendMailAsync(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email sending failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendEmailWithTemplateAsync(
            string serviceName,
            string to,
            string recipientName,
            Dictionary<string, string> templateData,
            int? relatedEntityId = null,
            string? relatedEntityType = null)
        {
            try
            {
                // Get email configuration by service name
                var config = await _emailConfigRepository.GetByServiceNameAsync(serviceName);

                if (config == null || !config.IsActive)
                {
                    throw new InvalidOperationException($"Email configuration '{serviceName}' not found or inactive");
                }

                // Replace placeholders in subject and body
                var subject = ReplacePlaceholders(config.EmailSubject, templateData);
                var body = ReplacePlaceholders(config.EmailBodyTemplate, templateData);

                // Send email
                var isSent = await SendEmailAsync(to, subject, body);

                // Log the email
                var emailLog = new EmailLog
                {
                    EmailConfigurationId = config.Id,
                    RecipientEmail = to,
                    RecipientName = recipientName,
                    Subject = subject,
                    Body = body,
                    IsSent = isSent,
                    ErrorMessage = isSent ? null : "Failed to send email",
                    RelatedEntityId = relatedEntityId,
                    RelatedEntityType = relatedEntityType,
                    SentAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                await _emailLogRepository.AddAsync(emailLog);

                return isSent;
            }
            catch (Exception ex)
            {
                // Log error
                var emailLog = new EmailLog
                {
                    EmailConfigurationId = 0,
                    RecipientEmail = to,
                    RecipientName = recipientName,
                    Subject = serviceName,
                    Body = string.Empty,
                    IsSent = false,
                    ErrorMessage = ex.Message,
                    RelatedEntityId = relatedEntityId,
                    RelatedEntityType = relatedEntityType,
                    SentAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                await _emailLogRepository.AddAsync(emailLog);
                return false;
            }
        }

        private string ReplacePlaceholders(string template, Dictionary<string, string> data)
        {
            var result = template;
            foreach (var kvp in data)
            {
                result = Regex.Replace(result, $@"\{{{kvp.Key}\}}", kvp.Value, RegexOptions.IgnoreCase);
            }
            return result;
        }
    }
}
