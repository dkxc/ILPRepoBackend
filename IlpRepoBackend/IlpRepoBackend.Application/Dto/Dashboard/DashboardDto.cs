using IlpRepoBackend.Domain.Enum;

namespace IlpRepoBackend.Application.Dto.Dashboard
{
    public class TraineeDashboardDto
    {
        public ProfileDto Profile { get; set; }
        public ProjectDto? Project { get; set; }
        public BatchDto Batch { get; set; }
        public ScoresDto Scores { get; set; }
        public List<SessionDto> Sessions { get; set; }
        public List<DocumentDto> Documents { get; set; }
    }

    public class ProfileDto
    {
        public string FirstName { get; set; }
        public int? ProjectId { get; set; }
        public int BatchId { get; set; }
    }

    public class ProjectDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public ProjectStatus Status { get; set; }
        public List<string> Technologies { get; set; } = new();
        public TeamDto Team { get; set; } = new();
        public int Progress { get; set; }
    }

    public class TeamDto
    {
        public int Number { get; set; }
        public List<string> Members { get; set; } = new();
    }

    public class BatchDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Day { get; set; }
        public string Status { get; set; }
    }

    public class ScoresDto
    {
        public double Average { get; set; }
        public int Rank { get; set; }
        public List<CourseScoreDto> Courses { get; set; } = new();
    }

    public class CourseScoreDto
    {
        public string Caption { get; set; }
        public double Value { get; set; }
    }

    public class SessionDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public DateTime Date { get; set; }
    }

    public class DocumentDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime UploadDate { get; set; }
        public string Type { get; set; }
        public string? Url { get; set; }
    }
}
