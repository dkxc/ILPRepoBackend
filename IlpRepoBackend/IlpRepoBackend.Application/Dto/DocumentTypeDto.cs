namespace IlpRepoBackend.Application.Dto
{
    public class DocumentTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Type { get; set; } // File type: PDF, Excel, Word, PowerPoint, etc.
    }
}