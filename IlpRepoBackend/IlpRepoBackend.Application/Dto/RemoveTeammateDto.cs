using System.ComponentModel.DataAnnotations;

namespace IlpRepoBackend.Application.Dto
{
    public class RemoveTeammateDto
    {
        [Required]
        public int ProjectId { get; set; }
        
        [Required]
        public int TraineeId { get; set; }
    }
}