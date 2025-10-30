using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace IlpRepoBackend.Application.Dto
{
    public class CreateDocumentRequestDto
    {
        [Required]
        public int BatchId { get; set; }
        
        [Required]
        public int DocumentId { get; set; }
        
        [Required]
        public DateTime DueDate { get; set; }
        
        public IFormFile? File { get; set; }
    }
}