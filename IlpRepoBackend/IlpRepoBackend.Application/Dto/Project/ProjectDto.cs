using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public List<Mentor> Mentors { get; set; } = new();
        public List<Poc> Pocs { get; set; } = new();
        public List<DocumentRequest> documentRequest { get; set; } = new();
        public List<DocumentSubmission> documentSubmissions { get; set; } = new();

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? TeamLead { get; internal set; }
        public string? ScrumMaster { get; internal set; }
    }

}
