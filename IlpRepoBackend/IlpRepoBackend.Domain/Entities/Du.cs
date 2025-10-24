using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Domain.Entities
{
    public class Du
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<Buddy> Buddies { get; set; } = new List<Buddy>();
        public ICollection<TraineeDu> TraineeDus { get; set; } = new List<TraineeDu>();
    }
}
