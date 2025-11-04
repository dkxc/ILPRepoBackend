using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly IEmailConfigurationRepository _emailConfigRepository;
        private readonly IEmailLogRepository _emailLogRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IEmailConfigurationRepository emailConfigRepository,
            IEmailLogRepository emailLogRepository,
            IConfiguration configuration,
            ILogger<EmailService> logger)
        {
            _emailConfigRepository = emailConfigRepository;
            _emailLogRepository = emailLogRepository;
            _configuration = configuration;
            _logger = logger;
        }

        // ✅ Validate SMTP Credentials before sending any mail
        private async Task<bool> ValidateSmtpCredentialsAsync()
        {
            try
            {
                var smtpHost = _configuration["Email:SmtpHost"];
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var smtpUsername = _configuration["Email:SmtpUsername"];
                var smtpPassword = _configuration["Email:SmtpPassword"];

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                    EnableSsl = true
                };

                _logger.LogInformation("Validating SMTP credentials...");

                // Test authentication by sending a dummy email to self
                var testMail = new MailMessage(smtpUsername, smtpUsername)
                {
                    Subject = "SMTP Credential Test",
                    Body = "Testing SMTP authentication."
                };

                await client.SendMailAsync(testMail);

                _logger.LogInformation("✅ SMTP credentials verified successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ SMTP credential validation failed");
                return false;
            }
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                // ✅ Validate credentials before sending
                if (!await ValidateSmtpCredentialsAsync())
                {
                    _logger.LogError("SMTP credentials are invalid. Aborting email send.");
                    return false;
                }

                _logger.LogInformation("Sending email to: {Email}", to);

                var smtpHost = _configuration["Email:SmtpHost"];
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var smtpUsername = _configuration["Email:SmtpUsername"];
                var smtpPassword = _configuration["Email:SmtpPassword"];
                var fromEmail = _configuration["Email:FromEmail"];
                var fromName = _configuration["Email:FromName"];

                _logger.LogDebug("SMTP Config → Host:{Host}, Port:{Port}, User:{User}", smtpHost, smtpPort, smtpUsername);

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

                _logger.LogInformation("Sending email via SMTP...");
                await client.SendMailAsync(mailMessage);

                _logger.LogInformation("✅ Email sent successfully to {Email}", to);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Email sending failed to {Email}", to);
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
                _logger.LogInformation("Fetching email config for service: {Service}", serviceName);

                var config = await _emailConfigRepository.GetByServiceNameAsync(serviceName);

                if (config == null || !config.IsActive)
                {
                    _logger.LogWarning("Email config for service {Service} not found or inactive", serviceName);
                    throw new InvalidOperationException($"Email configuration '{serviceName}' not found or inactive");
                }

                _logger.LogInformation("Replacing template placeholders");
                var subject = ReplacePlaceholders(config.EmailSubject, templateData);
                var body = ReplacePlaceholders(config.EmailBodyTemplate, templateData);

                _logger.LogDebug("Processed Subject: {Subject}", subject);
                _logger.LogDebug("Processed Body: {Body}", body);

                var isSent = await SendEmailAsync(to, subject, body);

                _logger.LogInformation("Saving email log...");
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
                _logger.LogError(ex, "❌ Error sending template email for {Service}", serviceName);

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
                _logger.LogDebug("Replacing {Key}", kvp.Key);
                result = Regex.Replace(result, $@"\{{{kvp.Key}\}}", kvp.Value, RegexOptions.IgnoreCase);
            }
            return result;
        }
    }
}
