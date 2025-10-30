namespace IlpRepoBackend.Application.Dto
{
    public class TraineeDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNo { get; set; }
        public string Role { get; set; } = string.Empty;
    }
}
