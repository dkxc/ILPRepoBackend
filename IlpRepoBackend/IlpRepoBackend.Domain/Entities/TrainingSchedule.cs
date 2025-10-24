using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Domain.Entities
{
    public class TrainingSchedule
    {
        public int Id { get; set; }
        public int? BatchId { get; set; }
        public DateTime TrainingDate { get; set; }
        public int Hours { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Batch? Batch { get; set; }
    }
}
