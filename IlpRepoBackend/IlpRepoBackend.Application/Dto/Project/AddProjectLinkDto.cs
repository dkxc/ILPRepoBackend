namespace IlpRepoBackend.Application.Dto.Project
{
    public class AddProjectLinkDto
    {
        public int LinkId { get; set; } // Link type ID
        public string LinkUrl { get; set; } = string.Empty;
    }
}