namespace IlpRepoBackend.Application.Dto
{
    public class AddBoPhaseForBatchDto
    {
        public string TraineeName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string BuddyName { get; set; } = string.Empty;
        public string? DuName { get; set; }  // Added DU name
    }
}