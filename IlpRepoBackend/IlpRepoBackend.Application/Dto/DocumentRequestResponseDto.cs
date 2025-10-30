namespace IlpRepoBackend.Application.Dto
{
    public class DocumentRequestResponseDto
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public int DocumentId { get; set; }
        public string DocumentName { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public DateTime DueDate { get; set; }
        public string? FileUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}