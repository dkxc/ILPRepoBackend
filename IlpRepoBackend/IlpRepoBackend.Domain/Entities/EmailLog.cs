using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Domain.Entities
{
    public class EmailLog
    {
        public int Id { get; set; }
        public int EmailConfigurationId { get; set; }
        public string RecipientEmail { get; set; } = string.Empty;
        public string RecipientName { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsSent { get; set; }
        public string? ErrorMessage { get; set; }
        public int? RelatedEntityId { get; set; } // e.g., DocumentRequestId
        public string? RelatedEntityType { get; set; } // e.g., "DocumentRequest"
        public DateTime SentAt { get; set; }
        public DateTime CreatedAt { get; set; }

        public EmailConfiguration? EmailConfiguration { get; set; }
    }
}
