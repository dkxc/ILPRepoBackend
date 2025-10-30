using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Domain.Entities
{
    public class DocumentSubmission
    {
        public int Id { get; set; }
        public int? RequestId { get; set; } // Which document request this fulfills
        public int? DocumentId { get; set; } // Which document type (BRD, Sprint Tracker, etc.)
        public int? ProjectId { get; set; } // Which project this submission is for (redundant but useful for queries)
        public int? TraineeId { get; set; } // WHO submitted this document
        public string? SubmissionLink { get; set; } // URL/path to submitted file
        public string? FileName { get; set; } // Original file name (e.g., "project_brd_final.pdf")
        public string? FileType { get; set; } // File extension/type (e.g., "PDF", "Excel")
        public DateTime SubmissionDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public DocumentRequest? DocumentRequest { get; set; }
        public Documents? Document { get; set; }
        public Project? Project { get; set; }
        public Trainee? Trainee { get; set; }
    }
}
