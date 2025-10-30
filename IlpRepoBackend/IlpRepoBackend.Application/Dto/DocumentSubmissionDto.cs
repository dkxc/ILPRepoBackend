namespace IlpRepoBackend.Application.Dto
{
    public class DocumentSubmissionDto
    {
        public int Id { get; set; }
        public string? SubmissionLink { get; set; }
        public string? FileName { get; set; }
        public string? FileType { get; set; }
        public string DocumentName { get; set; } = string.Empty; // e.g., "BRD", "Sprint Tracker"
        public int? ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public int? TraineeId { get; set; }
        public string? TraineeName { get; set; }
        public DateTime SubmissionDate { get; set; }
    }
}