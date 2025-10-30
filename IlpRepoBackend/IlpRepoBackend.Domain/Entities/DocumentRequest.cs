using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Domain.Entities
{
    public class DocumentRequest
    {
        public int Id { get; set; }
        public int? ProjectId { get; set; }
        public int? DocumentId { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime DueDate { get; set; }
        public string? FileUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Project? Project { get; set; }
        public Documents? Document { get; set; }
        public ICollection<DocumentSubmission> DocumentSubmissions { get; set; } = new List<DocumentSubmission>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
