namespace IlpRepoBackend.Application.Dto.DocumentSubmissions
{
    public class ProjectDocumentSubmissionInfo
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public string? SubmissionLink { get; set; }
        public DateTime SubmissionDate { get; set; }
        public int DocumentTypeId { get; set; }
        public string DocumentTypeName { get; set; } = string.Empty;
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public bool IsLateSubmission => SubmissionDate > DueDate;
    }
}