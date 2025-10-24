using IlpRepoBackend.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Domain.Entities
{
    public class Feedback
    {
        public int Id { get; set; }
        public int TraineeId { get; set; }
        public AssessmentType? AssessmentType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Trainee Trainee { get; set; } = null!;
        public ICollection<FeedbackHeaderResponse> FeedbackHeaderResponses { get; set; } = new List<FeedbackHeaderResponse>();
    }
}
