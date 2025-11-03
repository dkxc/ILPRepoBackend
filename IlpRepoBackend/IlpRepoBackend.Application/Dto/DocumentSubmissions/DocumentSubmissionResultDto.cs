namespace IlpRepoBackend.Application.Dto.DocumentSubmissions
{
    public class DocumentSubmissionResultDto
    {
        public int SubmissionId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public string SubmissionLink { get; set; } = string.Empty;
        public DateTime SubmissionDate { get; set; }
        public string DocumentTypeName { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public bool IsLateSubmission { get; set; }
    }
}