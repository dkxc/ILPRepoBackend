using IlpRepoBackend.Domain.Enum;
using IlpRepoBackend.Domain.Entities;
using System.Collections.Generic;

namespace IlpRepoBackend.Application.Dto.Project
{
    public class CreateProjectDto
    {
        public int BatchId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string? Technology { get; set; }
        public ProjectStatus Status { get; set; }
        public int Progress { get; set; } = 0;

        // Team members
        public List<string> TeamMembers { get; set; } = new();

        // NEW: Names only
        public string? TeamLeadName { get; set; }
        public string? ScrumMasterName { get; set; }

        // Mentors and POCs
        public List<Mentor> Mentors { get; set; } = new();
        public List<Poc> Pocs { get; set; } = new();
    }
}
