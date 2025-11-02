using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Enum;
using System;
using System.Collections.Generic;

namespace IlpRepoBackend.Application.Dto.Project
{
    public class ProjectDto
    {
        public int Id { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public ProjectStatus Status { get; set; }
        public int Progress { get; set; }
        public string? Technology { get; set; }
        public int BatchId { get; set; }
        public string BatchName { get; set; } = string.Empty;
        public List<string> TeamMembers { get; set; } = new();
        public List<MentorDto> Mentors { get; set; } = new();
        public List<PocDto> Pocs { get; set; } = new();
        public List<DocumentRequestDto> DocumentRequests { get; set; } = new(); // Changed from documentRequest
        public List<DocumentSubmissionDto> DocumentSubmissions { get; set; } = new(); // Changed from documentSubmissions
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? TeamLead { get; set; }
        public string? ScrumMaster { get; set; }
    }

    public class MentorDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public MentorType MentorType { get; set; }
    }

    public class PocDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
    }

    public class DocumentRequestDto
    {
        public int Id { get; set; }
        public int? DocumentId { get; set; }
        public string? DocumentName { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime DueDate { get; set; }
    }

    public class DocumentSubmissionDto
    {
        public int Id { get; set; }
        public int? DocumentId { get; set; }
        public int? RequestId { get; set; }
        public string? FileName { get; set; }
        public string? FileType { get; set; }
        public string? SubmissionLink { get; set; }
        public DateTime SubmissionDate { get; set; }
    }
}