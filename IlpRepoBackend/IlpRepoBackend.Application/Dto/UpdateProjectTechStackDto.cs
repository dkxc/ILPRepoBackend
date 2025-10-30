using System.ComponentModel.DataAnnotations;

namespace IlpRepoBackend.Application.Dto
{
    public class UpdateProjectTechStackDto
    {
        [Required]
        public int ProjectId { get; set; }
        
        public List<string> TechStack { get; set; } = new();
    }
}