using IlpRepoBackend.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Domain.Entities
{
    public class Batch
    {
        public int Id { get; set; }
        public string BatchName { get; set; } = string.Empty;
        // replaced string BatchType with FK to BatchType
        public int? BatchTypeId { get; set; }
        public BatchType? BatchType { get; set; }
        public BatchStatus Status { get; set; } = BatchStatus.NotStarted;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<Trainee> Trainees { get; set; } = new List<Trainee>();
        public ICollection<TrainingSchedule> TrainingSchedules { get; set; } = new List<TrainingSchedule>();
        public ICollection<Phase> Phases { get; set; } = new List<Phase>();
    }
}
