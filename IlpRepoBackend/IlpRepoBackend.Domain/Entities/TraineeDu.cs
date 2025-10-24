using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Domain.Entities
{
    public class TraineeDu
    {
        public int Id { get; set; }
        public int? TraineeId { get; set; }
        public int? DuId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public string Location { get; set; }
        
        public string ojtMenter { get; set; }
        public Trainee? Trainee { get; set; }
        public Du? Du { get; set; }
    }
}
