using IlpRepoBackend.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Domain.Entities
{
    public class Project
    {
        public int Id { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public ProjectStatus Status { get; set; } = ProjectStatus.NotLive;
        public int Progress { get; set; } = 0;
        public string? Technology { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<ProjectTeam> ProjectTeams { get; set; } = new List<ProjectTeam>();
        public ICollection<PocsForProject> PocsForProjects { get; set; } = new List<PocsForProject>();

        public ICollection<MenterForAProject> MentersForProjects { get; set; } = new List<MenterForAProject>();
        public ICollection<ProjectLink> ProjectLinks { get; set; } = new List<ProjectLink>();
        public ICollection<DocumentRequest> DocumentRequests { get; set; } = new List<DocumentRequest>();
        //public ICollection<MenterForAProject> MentersForProjects { get; set; } = new List<MenterForAProject>();
        //public ICollection<PocsForProject> PocsForProjects { get; set; } = new List<PocsForProject>();

    }
}
