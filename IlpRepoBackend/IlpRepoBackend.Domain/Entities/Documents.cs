using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Domain.Entities
{
    /// <summary>
    /// Master catalog of document types (BRD, Sprint Tracker, etc.)
    /// This table defines what document types exist in the system
    /// </summary>
    public class Documents
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // e.g., "BRD", "Sprint Tracker", "Test Cases"
        public string? Type { get; set; } // Expected file type: "PDF", "Excel", "Word", "PowerPoint"
        public string? TemplateLink { get; set; } // Link to downloadable template for this document type
        public DateTime UploadDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<DocumentRequest> DocumentRequests { get; set; } = new List<DocumentRequest>();
        public ICollection<DocumentSubmission> DocumentSubmissions { get; set; } = new List<DocumentSubmission>();
    }
}
