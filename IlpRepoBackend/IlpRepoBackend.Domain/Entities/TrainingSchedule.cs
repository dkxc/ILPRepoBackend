namespace IlpRepoBackend.Domain.Entities
{
    public class TrainingSchedule
    {
        public int Id { get; set; }
        public int? BatchId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTime TrainingDate { get; set; }
        public int Hours { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        // ✅ Navigation property
        public virtual Batch? Batch { get; set; }
    }
}


