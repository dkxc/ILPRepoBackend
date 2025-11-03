namespace IlpRepoBackend.Application.Dto.Links
{
    public class BatchLinkTypeDto
    {
        public int LinkTypeId { get; set; }
        public string LinkTypeName { get; set; } = string.Empty;
        public int TotalProjects { get; set; } // Total projects in batch that should have this link type
        public int SubmittedProjects { get; set; } // Projects that have submitted this link type
        public int PendingProjects { get; set; } // Projects that haven't submitted this link type
        public double CompletionPercentage { get; set; } // Percentage of projects that submitted
    }
}