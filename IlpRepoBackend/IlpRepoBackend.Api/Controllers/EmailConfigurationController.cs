using IlpRepoBackend.Application.DTOs.EmailConfiguration;
using IlpRepoBackend.Domain.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IlpRepoBackend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Add appropriate authorization
    public class EmailConfigurationController : ControllerBase
    {
        private readonly IEmailConfigurationRepository _repository;
        private readonly ILogger<EmailConfigurationController> _logger;

        public EmailConfigurationController(
            IEmailConfigurationRepository repository,
            ILogger<EmailConfigurationController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        /// <summary>
        /// Get all email configurations
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var configs = await _repository.GetAllAsync();
                var configDtos = configs.Select(c => new EmailConfigurationDto
                {
                    Id = c.Id,
                    ServiceName = c.ServiceName,
                    ServiceDescription = c.ServiceDescription,
                    DaysBeforeDueDate = c.DaysBeforeDueDate,
                    ScheduledTime = c.ScheduledTime,
                    EmailSubject = c.EmailSubject,
                    EmailBodyTemplate = c.EmailBodyTemplate,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                });

                return Ok(new { success = true, data = configDtos });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving email configurations");
                return StatusCode(500, new { success = false, message = "Error retrieving configurations" });
            }
        }

        /// <summary>
        /// Get email configuration by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var config = await _repository.GetByIdAsync(id);

                if (config == null)
                {
                    return NotFound(new { success = false, message = $"Email configuration with ID {id} not found" });
                }

                var configDto = new EmailConfigurationDto
                {
                    Id = config.Id,
                    ServiceName = config.ServiceName,
                    ServiceDescription = config.ServiceDescription,
                    DaysBeforeDueDate = config.DaysBeforeDueDate,
                    ScheduledTime = config.ScheduledTime,
                    EmailSubject = config.EmailSubject,
                    EmailBodyTemplate = config.EmailBodyTemplate,
                    IsActive = config.IsActive,
                    CreatedAt = config.CreatedAt,
                    UpdatedAt = config.UpdatedAt
                };

                return Ok(new { success = true, data = configDto });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving email configuration {id}");
                return StatusCode(500, new { success = false, message = "Error retrieving configuration" });
            }
        }

        /// <summary>
        /// Get email configuration by service name
        /// </summary>
        [HttpGet("service/{serviceName}")]
        public async Task<IActionResult> GetByServiceName(string serviceName)
        {
            try
            {
                var config = await _repository.GetByServiceNameAsync(serviceName);

                if (config == null)
                {
                    return NotFound(new { success = false, message = $"Email configuration for service '{serviceName}' not found" });
                }

                var configDto = new EmailConfigurationDto
                {
                    Id = config.Id,
                    ServiceName = config.ServiceName,
                    ServiceDescription = config.ServiceDescription,
                    DaysBeforeDueDate = config.DaysBeforeDueDate,
                    ScheduledTime = config.ScheduledTime,
                    EmailSubject = config.EmailSubject,
                    EmailBodyTemplate = config.EmailBodyTemplate,
                    IsActive = config.IsActive,
                    CreatedAt = config.CreatedAt,
                    UpdatedAt = config.UpdatedAt
                };

                return Ok(new { success = true, data = configDto });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving email configuration for service {serviceName}");
                return StatusCode(500, new { success = false, message = "Error retrieving configuration" });
            }
        }

        /// <summary>
        /// Update email configuration
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateEmailConfigurationDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });
                }

                var existingConfig = await _repository.GetByIdAsync(id);

                if (existingConfig == null)
                {
                    return NotFound(new { success = false, message = $"Email configuration with ID {id} not found" });
                }

                // Update only the editable fields
                existingConfig.DaysBeforeDueDate = updateDto.DaysBeforeDueDate;
                existingConfig.EmailSubject = updateDto.EmailSubject;
                existingConfig.EmailBodyTemplate = updateDto.EmailBodyTemplate;
                existingConfig.IsActive = updateDto.IsActive;
                existingConfig.UpdatedAt = DateTime.UtcNow;

                // Optional: Update scheduled time if provided
                if (updateDto.ScheduledTime.HasValue)
                {
                    existingConfig.ScheduledTime = updateDto.ScheduledTime.Value;
                }

                await _repository.UpdateAsync(existingConfig);

                _logger.LogInformation($"Email configuration {id} ({existingConfig.ServiceName}) updated successfully");

                var configDto = new EmailConfigurationDto
                {
                    Id = existingConfig.Id,
                    ServiceName = existingConfig.ServiceName,
                    ServiceDescription = existingConfig.ServiceDescription,
                    DaysBeforeDueDate = existingConfig.DaysBeforeDueDate,
                    ScheduledTime = existingConfig.ScheduledTime,
                    EmailSubject = existingConfig.EmailSubject,
                    EmailBodyTemplate = existingConfig.EmailBodyTemplate,
                    IsActive = existingConfig.IsActive,
                    CreatedAt = existingConfig.CreatedAt,
                    UpdatedAt = existingConfig.UpdatedAt
                };

                return Ok(new { success = true, message = "Configuration updated successfully", data = configDto });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating email configuration {id}");
                return StatusCode(500, new { success = false, message = "Error updating configuration" });
            }
        }

        /// <summary>
        /// Toggle configuration active status
        /// </summary>
        [HttpPatch("{id}/toggle-active")]
        public async Task<IActionResult> ToggleActive(int id)
        {
            try
            {
                var config = await _repository.GetByIdAsync(id);

                if (config == null)
                {
                    return NotFound(new { success = false, message = $"Email configuration with ID {id} not found" });
                }

                config.IsActive = !config.IsActive;
                config.UpdatedAt = DateTime.UtcNow;

                await _repository.UpdateAsync(config);

                _logger.LogInformation($"Email configuration {id} ({config.ServiceName}) active status toggled to {config.IsActive}");

                return Ok(new
                {
                    success = true,
                    message = $"Configuration {(config.IsActive ? "activated" : "deactivated")} successfully",
                    data = new { id = config.Id, isActive = config.IsActive }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error toggling active status for configuration {id}");
                return StatusCode(500, new { success = false, message = "Error updating configuration" });
            }
        }

        /// <summary>
        /// Get all active configurations
        /// </summary>
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveConfigurations()
        {
            try
            {
                var configs = await _repository.GetActiveConfigurationsAsync();
                var configDtos = configs.Select(c => new EmailConfigurationDto
                {
                    Id = c.Id,
                    ServiceName = c.ServiceName,
                    ServiceDescription = c.ServiceDescription,
                    DaysBeforeDueDate = c.DaysBeforeDueDate,
                    ScheduledTime = c.ScheduledTime,
                    EmailSubject = c.EmailSubject,
                    EmailBodyTemplate = c.EmailBodyTemplate,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                });

                return Ok(new { success = true, data = configDtos });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active email configurations");
                return StatusCode(500, new { success = false, message = "Error retrieving configurations" });
            }
        }

        /// <summary>
        /// Create new email configuration
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEmailConfigurationDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });
                }

                // Check if service name already exists
                var existingConfig = await _repository.GetByServiceNameAsync(createDto.ServiceName);
                if (existingConfig != null)
                {
                    return BadRequest(new { success = false, message = $"Email configuration with service name '{createDto.ServiceName}' already exists" });
                }

                var newConfig = new Domain.Entities.EmailConfiguration
                {
                    ServiceName = createDto.ServiceName,
                    ServiceDescription = createDto.ServiceDescription,
                    DaysBeforeDueDate = createDto.DaysBeforeDueDate,
                    ScheduledTime = createDto.ScheduledTime,
                    EmailSubject = createDto.EmailSubject,
                    EmailBodyTemplate = createDto.EmailBodyTemplate,
                    IsActive = createDto.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _repository.AddAsync(newConfig);

                _logger.LogInformation($"Email configuration created successfully: {newConfig.ServiceName} (ID: {newConfig.Id})");

                var configDto = new EmailConfigurationDto
                {
                    Id = newConfig.Id,
                    ServiceName = newConfig.ServiceName,
                    ServiceDescription = newConfig.ServiceDescription,
                    DaysBeforeDueDate = newConfig.DaysBeforeDueDate,
                    ScheduledTime = newConfig.ScheduledTime,
                    EmailSubject = newConfig.EmailSubject,
                    EmailBodyTemplate = newConfig.EmailBodyTemplate,
                    IsActive = newConfig.IsActive,
                    CreatedAt = newConfig.CreatedAt,
                    UpdatedAt = newConfig.UpdatedAt
                };

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = newConfig.Id },
                    new { success = true, message = "Configuration created successfully", data = configDto }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating email configuration");
                return StatusCode(500, new { success = false, message = "Error creating configuration" });
            }
        }
    }
}