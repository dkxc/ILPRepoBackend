using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace IlpRepoBackend.Application.Dto
{
    public class UpdateDocumentRequestDto
    {
        [Required]
        public int Id { get; set; }
        
        [Required]
        public int BatchId { get; set; }
        
        [Required]
        public int DocumentId { get; set; }
        
        [Required]
        public DateTime DueDate { get; set; }
        
        public IFormFile? File { get; set; }
        
        public bool RemoveExistingFile { get; set; } = false;
    }
}