using System.ComponentModel.DataAnnotations;

namespace IlpRepoBackend.Application.Dto
{
    public class UpdateProjectLinksDto
    {
        [Required]
        public int ProjectId { get; set; }

        [Required]
        public int TraineeId { get; set; } // WHO is making this change

        [Required]
        public List<UpdateProjectLinkDto> ProjectLinks { get; set; } = new();
    }
}