using IlpRepoBackend.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Domain.Entities
{
    public class Mentor
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int? ProjectId { get; set; }
        public MentorType MentorType { get; set; } // NEW PROPERTY
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Project? Project { get; set; }
    }
}
