namespace IlpRepoBackend.Application.Dto
{
    public class DocumentLinkDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Type { get; set; }
        public string? TemplateLink { get; set; } // Link to downloadable template
    }
}