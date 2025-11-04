using IlpRepoBackend.Domain.Persistence;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Services
{
    public class PasswordSetupEmailService : IPasswordSetupEmailService
    {
        private readonly IEmailService _emailService;
        private readonly IEmailConfigurationRepository _emailConfigRepository;
        private readonly ILogger<PasswordSetupEmailService> _logger;
        private const int ConfigId = 2; // Fixed ID for PasswordSetup service

        public PasswordSetupEmailService(
            IEmailService emailService,
            IEmailConfigurationRepository emailConfigRepository,
            ILogger<PasswordSetupEmailService> logger)
        {
            _emailService = emailService;
            _emailConfigRepository = emailConfigRepository;
            _logger = logger;
        }

        public async Task<bool> SendPasswordSetupOtpAsync(string email, string otp, string userName)
        {
            try
            {
                var config = await _emailConfigRepository.GetByIdAsync(ConfigId);
                if (config == null || !config.IsActive)
                {
                    _logger.LogWarning("Password setup email configuration not found or inactive");
                    return false;
                }

                var templateData = new Dictionary<string, string>
                {
                    { "RecipientName", userName },
                    { "OtpCode", otp },
                    { "ExpiryMinutes", "10" }
                };

                var emailSent = await _emailService.SendEmailWithTemplateAsync(
                    "PasswordSetup",
                    email,
                    userName,
                    templateData,
                    ConfigId,
                    "PasswordSetup"
                );

                return emailSent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password setup OTP email");
                return false;
            }
        }
    }
}
