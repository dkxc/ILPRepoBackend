using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Domain.Entities
{
    public class FeedbackHeaderResponse
    {
        public int Id { get; set; }
        public int? FeedbackHeaderId { get; set; }
        public int? FeedbackId { get; set; }
        public string? Text { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public FeedbackHeader? FeedbackHeader { get; set; }
        public Feedback? Feedback { get; set; }
    }
}
