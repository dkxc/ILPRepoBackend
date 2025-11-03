using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace IlpRepoBackend.Application.Dto.DocumentSubmissions
{
    public class SubmitDocumentDto
    {
        [Required]
        public int DocumentRequestId { get; set; }
        
        [Required]
        public IFormFile DocumentFile { get; set; } = null!;
    }
}