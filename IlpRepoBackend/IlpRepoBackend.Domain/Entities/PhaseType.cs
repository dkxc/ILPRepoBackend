using System;
using System.Collections.Generic;

namespace IlpRepoBackend.Domain.Entities
{
    public class PhaseType
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<Phase> Phases { get; set; } = new List<Phase>();
    }
}