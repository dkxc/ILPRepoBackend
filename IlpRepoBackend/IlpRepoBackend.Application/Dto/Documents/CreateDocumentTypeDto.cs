using System.ComponentModel.DataAnnotations;

namespace IlpRepoBackend.Application.Dto.Documents
{
    public class CreateDocumentTypeDto
    {
        [Required]
        [StringLength(100, ErrorMessage = "Document type name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(50, ErrorMessage = "File type cannot exceed 50 characters")]
        public string? FileType { get; set; }
    }
}