using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Domain.Entities
{
    public class Notification
    {
        public int Id { get; set; }
        public DateTime? NotificationDate { get; set; }
        public int? DocumentRequestId { get; set; }
        public string? Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public DocumentRequest? DocumentRequest { get; set; }
    }
}
