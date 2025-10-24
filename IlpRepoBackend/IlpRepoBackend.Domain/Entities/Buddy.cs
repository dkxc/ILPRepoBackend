using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Domain.Entities
{
    public class Buddy
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int? DuId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Du? Du { get; set; }
        public ICollection<BoPhase> BoPhases { get; set; } = new List<BoPhase>();
    }
}
