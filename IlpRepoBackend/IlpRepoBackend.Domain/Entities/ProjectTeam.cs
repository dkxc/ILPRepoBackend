using IlpRepoBackend.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Domain.Entities
{
    public class ProjectTeam
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int TraineeId { get; set; }
        public ProjectRole Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Project Project { get; set; } = null!;
        public Trainee Trainee { get; set; } = null!;
    }
}
