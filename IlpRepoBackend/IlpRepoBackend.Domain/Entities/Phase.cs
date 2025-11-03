using System;

namespace IlpRepoBackend.Domain.Entities
{
    public class Phase
    {
        public int Id { get; set; }
        public string PhaseType { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public int BatchId { get; set; }
        public Batch Batch { get; set; } = null!;

        public int? PhaseTypeId { get; set; }
        public PhaseType? PhaseTypeEntity { get; set; }
    }
}