using IlpRepoBackend.Application.Services;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.BackgroundServices
{
    public class DocumentReminderBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DocumentReminderBackgroundService> _logger;
        private const int ConfigId = 1; // Fixed ID for DocumentRequestReminder service
        private const string ServiceName = "DocumentRequestReminder";

        public DocumentReminderBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<DocumentReminderBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"Document Reminder Background Service (ID: {ConfigId}) is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Get the scheduled time from database configuration
                    TimeSpan scheduledTime;

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var emailConfigRepository = scope.ServiceProvider.GetRequiredService<IEmailConfigurationRepository>();
                        var config = await emailConfigRepository.GetByIdAsync(ConfigId);

                        if (config == null)
                        {
                            _logger.LogError($"Email configuration with ID {ConfigId} not found. Service will retry in 1 hour.");
                            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                            continue;
                        }

                        if (!config.IsActive)
                        {
                            _logger.LogInformation($"Email configuration ID {ConfigId} is inactive. Service will check again in 1 hour.");
                            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                            continue;
                        }

                        scheduledTime = config.ScheduledTime;
                    }

                    var now = DateTime.Now;
                    var scheduledDateTime = now.Date + scheduledTime;

                    // If scheduled time has passed today, schedule for tomorrow
                    if (now > scheduledDateTime)
                    {
                        scheduledDateTime = scheduledDateTime.AddDays(1);
                    }

                    var delay = scheduledDateTime - now;

                    _logger.LogInformation($"Next reminder check scheduled at {scheduledDateTime} ({scheduledTime}). Waiting {delay.TotalHours:F2} hours.");

                    await Task.Delay(delay, stoppingToken);

                    if (!stoppingToken.IsCancellationRequested)
                    {
                        await CheckAndSendRemindersAsync();
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Document Reminder Background Service is being cancelled.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing document reminders.");
                    // Wait 1 hour before retrying in case of error
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }

            _logger.LogInformation("Document Reminder Background Service is stopping.");
        }

        private async Task CheckAndSendRemindersAsync()
        {
            using var scope = _serviceProvider.CreateScope();

            var emailConfigRepository = scope.ServiceProvider.GetRequiredService<IEmailConfigurationRepository>();
            var documentRequestRepository = scope.ServiceProvider.GetRequiredService<IDocumentRequestRepository>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var emailLogRepository = scope.ServiceProvider.GetRequiredService<IEmailLogRepository>();

            try
            {
                // Get email configuration by ID (fixed ID: 1)
                var config = await emailConfigRepository.GetByIdAsync(ConfigId);

                if (config == null || !config.IsActive)
                {
                    _logger.LogWarning($"{ServiceName} configuration (ID: {ConfigId}) not found or inactive.");
                    return;
                }

                var daysBeforeDueDate = config.DaysBeforeDueDate;
                var targetDate = DateTime.UtcNow.Date.AddDays(daysBeforeDueDate);

                _logger.LogInformation($"Checking for document requests due on {targetDate:yyyy-MM-dd} ({daysBeforeDueDate} days from now)");

                // Get pending requests using extended repository method
                var pendingRequests = await documentRequestRepository.GetPendingRequestsDueByDateAsync(targetDate);

                _logger.LogInformation($"Found {pendingRequests.Count()} document requests due in {daysBeforeDueDate} days.");

                foreach (var request in pendingRequests)
                {
                    try
                    {
                        // Check if reminder already sent today using extended method
                        var sentToday = await emailLogRepository.WasReminderSentTodayAsync("DocumentRequest", request.Id);

                        if (sentToday)
                        {
                            _logger.LogInformation($"Reminder already sent today for DocumentRequest {request.Id}");
                            continue;
                        }

                        // Get project details (already loaded via include in repository)
                        var project = request.Project;
                        if (project == null)
                        {
                            _logger.LogWarning($"Project not found for DocumentRequest {request.Id}");
                            continue;
                        }

                        // Get document name
                        var documentName = request.Document?.Name ?? "Unknown Document";

                        // Get team members emails
                        var teamMembers = project.ProjectTeams?
                            .Where(pt => pt.Trainee?.User != null)
                            .Select(pt => pt.Trainee.User)
                            .ToList() ?? new List<User>();

                        if (!teamMembers.Any())
                        {
                            _logger.LogWarning($"No team members found for project {project.Id}");
                            continue;
                        }

                        foreach (var member in teamMembers)
                        {
                            if (string.IsNullOrWhiteSpace(member.Email))
                            {
                                _logger.LogWarning($"User {member.Username} has no email address");
                                continue;
                            }

                            var templateData = new Dictionary<string, string>
                            {
                                { "RecipientName", member.Username ?? "Team Member" },
                                { "ProjectName", project.ProjectName },
                                { "DocumentName", documentName },
                                { "DueDate", request.DueDate.ToString("MMMM dd, yyyy") },
                                { "DaysRemaining", daysBeforeDueDate.ToString() },
                                { "RequestDate", request.RequestDate.ToString("MMMM dd, yyyy") }
                            };

                            var emailSent = await emailService.SendEmailWithTemplateAsync(
                                ServiceName,
                                member.Email,
                                member.Username ?? "Team Member",
                                templateData,
                                request.Id,
                                "DocumentRequest"
                            );

                            if (emailSent)
                            {
                                _logger.LogInformation($"Reminder sent to {member.Email} for DocumentRequest {request.Id}");
                            }
                            else
                            {
                                _logger.LogError($"Failed to send reminder to {member.Email} for DocumentRequest {request.Id}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error sending reminder for DocumentRequest {request.Id}");
                    }
                }

                _logger.LogInformation("Document reminder check completed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CheckAndSendRemindersAsync");
            }
        }
    }
}