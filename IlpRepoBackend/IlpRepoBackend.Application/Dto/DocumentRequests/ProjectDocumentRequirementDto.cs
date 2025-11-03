namespace IlpRepoBackend.Application.Dto.DocumentRequests
{
    public class ProjectDocumentRequirementDto
    {
        public int DocumentRequestId { get; set; }
        public int DocumentTypeId { get; set; }
        public string DocumentTypeName { get; set; } = string.Empty;
        public string? DocumentTemplateUrl { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public DateTime RequestDate { get; set; }
        public bool IsOverdue => DateTime.UtcNow > DueDate;
        public int DaysUntilDue => (DueDate - DateTime.UtcNow).Days;
        public string UrgencyLevel => GetUrgencyLevel();
        public bool HasSubmission { get; set; }
        public DateTime? LastSubmissionDate { get; set; }

        private string GetUrgencyLevel()
        {
            var daysUntilDue = DaysUntilDue;
            if (daysUntilDue < 0) return "Overdue";
            if (daysUntilDue <= 1) return "Critical";
            if (daysUntilDue <= 3) return "High";
            if (daysUntilDue <= 7) return "Medium";
            return "Low";
        }
    }
}