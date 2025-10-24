using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Domain.Entities
{
    public class Result
    {
        public int Id { get; set; }
        public int? TraineeId { get; set; }
        public int? AssessmentId { get; set; }
        public int ObtainedMark { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Trainee? Trainee { get; set; }
        public Assessment? Assessment { get; set; }
    }
}
