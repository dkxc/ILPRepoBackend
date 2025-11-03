namespace IlpRepoBackend.Application.Dto.Documents
{
    public class DocumentTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Link { get; set; }
        public DateTime UploadDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}