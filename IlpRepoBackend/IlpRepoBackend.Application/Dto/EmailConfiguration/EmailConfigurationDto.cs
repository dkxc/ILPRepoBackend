using System;
using System.ComponentModel.DataAnnotations;

namespace IlpRepoBackend.Application.DTOs.EmailConfiguration
{
    /// <summary>
    /// DTO for returning email configuration data
    /// </summary>
    public class EmailConfigurationDto
    {
        public int Id { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string ServiceDescription { get; set; } = string.Empty;
        public int DaysBeforeDueDate { get; set; }
        public TimeSpan ScheduledTime { get; set; }
        public string EmailSubject { get; set; } = string.Empty;
        public string EmailBodyTemplate { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO for updating email configuration
    /// Only editable fields are included
    /// </summary>
    public class UpdateEmailConfigurationDto
    {
        [Required]
        [Range(0, 365, ErrorMessage = "Days before due date must be between 0 and 365")]
        public int DaysBeforeDueDate { get; set; }

        [Required(ErrorMessage = "Email subject is required")]
        [StringLength(500, ErrorMessage = "Email subject cannot exceed 500 characters")]
        public string EmailSubject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email body template is required")]
        [StringLength(10000, ErrorMessage = "Email body template cannot exceed 10000 characters")]
        public string EmailBodyTemplate { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Optional: Update scheduled time if needed
        /// </summary>
        public TimeSpan? ScheduledTime { get; set; }
    }

    /// <summary>
    /// DTO for creating new email configuration (if needed)
    /// </summary>
    public class CreateEmailConfigurationDto
    {
        [Required(ErrorMessage = "Service name is required")]
        [StringLength(100, ErrorMessage = "Service name cannot exceed 100 characters")]
        public string ServiceName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Service description cannot exceed 500 characters")]
        public string ServiceDescription { get; set; } = string.Empty;

        [Required]
        [Range(0, 365, ErrorMessage = "Days before due date must be between 0 and 365")]
        public int DaysBeforeDueDate { get; set; }

        [Required]
        public TimeSpan ScheduledTime { get; set; } = new TimeSpan(9, 0, 0); // Default 9:00 AM

        [Required(ErrorMessage = "Email subject is required")]
        [StringLength(500, ErrorMessage = "Email subject cannot exceed 500 characters")]
        public string EmailSubject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email body template is required")]
        [StringLength(10000, ErrorMessage = "Email body template cannot exceed 10000 characters")]
        public string EmailBodyTemplate { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}