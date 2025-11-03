using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Domain.Entities
{
    public class Documents
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string FileType { get; set; }
        public string? Link { get; set; }
        public DateTime UploadDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<DocumentRequest> DocumentRequests { get; set; } = new List<DocumentRequest>();
        public ICollection<DocumentSubmission> DocumentSubmissions { get; set; } = new List<DocumentSubmission>();
    }
}
