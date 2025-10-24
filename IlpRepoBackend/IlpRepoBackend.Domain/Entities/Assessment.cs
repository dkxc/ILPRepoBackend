using IlpRepoBackend.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Domain.Entities
{
    public class Assessment
    {
        public int Id { get; set; }
        public AssessmentType Type { get; set; }
        public int MaxMark { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<Result> Results { get; set; } = new List<Result>();
    }
}
