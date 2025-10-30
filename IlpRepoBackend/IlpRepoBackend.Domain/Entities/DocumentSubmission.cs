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
        public int? DocumentId { get; set; }
        public int? RequestId { get; set; }
        public string? SubmissionLink { get; set; } // NEW PROPERTY
        public DateTime SubmissionDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Documents? Document { get; set; }
        public DocumentRequest? DocumentRequest { get; set; }
    }
}
