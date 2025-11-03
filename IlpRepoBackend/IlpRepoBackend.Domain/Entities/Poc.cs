using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace IlpRepoBackend.Domain.Entities
{
    public class Poc
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        [JsonIgnore]
        public ICollection<PocsForProject> PocsForProjects { get; set; } = new List<PocsForProject>();
    }

}
