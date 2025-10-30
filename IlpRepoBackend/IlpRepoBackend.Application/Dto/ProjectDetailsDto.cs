namespace IlpRepoBackend.Application.Dto
{
    public class ProjectDetailsDto
    {
        public int Id { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int Progress { get; set; }
        public string? Technology { get; set; }
        public List<string> BatchNames { get; set; } = new();
        public List<TraineeDto> Trainees { get; set; } = new();
        public int NoOfTrainees { get; set; }
        public List<string> TechStack { get; set; } = new();
        public List<ProjectLinkDto> ProjectLinks { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
