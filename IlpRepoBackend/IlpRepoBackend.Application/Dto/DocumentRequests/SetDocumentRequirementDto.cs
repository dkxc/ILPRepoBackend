using System.ComponentModel.DataAnnotations;

namespace IlpRepoBackend.Application.Dto.DocumentRequests
{
    public class SetDocumentRequirementDto
    {
        [Required]
        public int DocumentTypeId { get; set; }

        [Required]
        public int BatchId { get; set; }

        [Required]
        public DateTime DueDate { get; set; }
    }
}