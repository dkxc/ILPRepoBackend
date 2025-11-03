namespace IlpRepoBackend.Application.Dto
{
    public class TraineeTrainingDetailsDto
    {
        public int TraineeId { get; set; }
        public string TraineeName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string BatchName { get; set; } = string.Empty;
        public string? BuddyName { get; set; }
        public string? BuddyDU { get; set; }
        public string? OjtMentor { get; set; }
        public string? DuAllocated { get; set; }
        public string? Location { get; set; }
    }
}
