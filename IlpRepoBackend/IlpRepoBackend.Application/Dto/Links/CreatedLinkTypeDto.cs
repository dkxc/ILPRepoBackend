namespace IlpRepoBackend.Application.Dto.Links
{
    public class CreatedLinkTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}