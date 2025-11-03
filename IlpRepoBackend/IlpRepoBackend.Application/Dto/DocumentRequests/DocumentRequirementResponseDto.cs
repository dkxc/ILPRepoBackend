namespace IlpRepoBackend.Application.Dto.DocumentRequests
{
    public class DocumentRequirementResponseDto
    {
        public int DocumentTypeId { get; set; }
        public string DocumentTypeName { get; set; } = string.Empty;
        public int BatchId { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime RequestDate { get; set; }
        public List<int> ProjectIds { get; set; } = new List<int>();
        public int TotalProjectsAffected { get; set; }
    }
}