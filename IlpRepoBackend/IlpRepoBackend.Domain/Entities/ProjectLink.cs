using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Domain.Entities
{
    public class ProjectLink
    {
        public int Id { get; set; }
        public int? LinkId { get; set; }
        public int? ProjectId { get; set; }
        public string? LinkUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Link? Link { get; set; }
        public Project? Project { get; set; }
    }
}
