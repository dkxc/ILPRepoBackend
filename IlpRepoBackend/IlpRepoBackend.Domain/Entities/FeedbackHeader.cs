using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Domain.Entities
{
    public class FeedbackHeader
    {
        public int Id { get; set; }
        public string? Header { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<FeedbackHeaderResponse> FeedbackHeaderResponses { get; set; } = new List<FeedbackHeaderResponse>();
    }
}
