using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace IlpRepoBackend.Application.Dto
{
    public class UploadDocumentSubmissionDto
    {
        [Required]
        public int RequestId { get; set; }
        
        [Required]
        public int DocumentId { get; set; }
        
        [Required]
        public int TraineeId { get; set; } // WHO is submitting
        
        [Required]
        public IFormFile File { get; set; } = null!;
    }
}