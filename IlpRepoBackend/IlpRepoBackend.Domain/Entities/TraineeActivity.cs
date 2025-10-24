using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Domain.Entities
{
    public class TraineeActivity
    {
        public int Id { get; set; }
        public int? TraineeId { get; set; }
        public string? ActivityType { get; set; }
        public DateTime ActivityTime { get; set; }
        public string? Details { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Trainee? Trainee { get; set; }
    }
}
