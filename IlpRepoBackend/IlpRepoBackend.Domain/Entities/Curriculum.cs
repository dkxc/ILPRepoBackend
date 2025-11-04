namespace IlpRepoBackend.Domain.Entities
{
    public class Curriculum
    {
        public int Id { get; set; }
        public int BatchId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Instructor { get; set; }
        public string? Color { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation property
        public virtual Batch Batch { get; set; }
    }
}
