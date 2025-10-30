namespace IlpRepoBackend.Application.Dto
{
    public class UpdateProjectLinkDto
    {
        public int? Id { get; set; } // Null for new links, populated for existing links
        public string? LinkUrl { get; set; }
        public int? LinkId { get; set; } // Reference to Link entity
        public bool IsDeleted { get; set; } = false; // Mark for deletion
    }
}