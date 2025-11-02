using IlpRepoBackend.Domain.Enum;
using System;
using System.Collections.Generic;

namespace IlpRepoBackend.Application.Dto.Project
{
    public class ProjectDetailsDto
    {
        public int Id { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public ProjectStatus Status { get; set; }
        public int Progress { get; set; }
        public string TechnologyStack { get; set; } = string.Empty; // Technology as string for display and textbox population
        public List<TraineeInfoDto> Trainees { get; set; } = new();
        public List<ProjectLinkDetailDto> ProjectLinks { get; set; } = new();
        public List<DocumentSubmissionDto> DocumentSubmissions { get; set; } = new(); // Added document submissions
    }

    public class TraineeInfoDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
    }

    public class ProjectLinkDetailDto
    {
        public int Id { get; set; } // ProjectLink ID for editing/deletion
        public int? LinkId { get; set; } // Link type ID for dropdown population
        public string? LinkUrl { get; set; } // URL for display and textbox population
        public string? LinkTypeName { get; set; } // Link type name for display
    }

    public class DocumentSubmissionDto
    {
        public int Id { get; set; }
        public string? FileName { get; set; }
        public string? FileType { get; set; }
        public string? SubmissionLink { get; set; }
        public DateTime SubmissionDate { get; set; }
        public string? DocumentName { get; set; } // Name of the document being submitted
        public DateTime? RequestDueDate { get; set; } // Due date from DocumentRequest
    }
}
