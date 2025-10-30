namespace IlpRepoBackend.Application.Dto
{
    public class ProjectTechStackDto
    {
        public int Id { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public List<string> TechStack { get; set; } = new();
        public string? Technology { get; set; } // Raw comma-separated value
    }
}