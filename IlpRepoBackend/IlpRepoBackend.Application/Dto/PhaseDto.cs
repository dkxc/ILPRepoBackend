namespace IlpRepoBackend.Application.Dto
{
    public class PhaseDto
    {
        public int Id { get; set; }
        public string PhaseType { get; set; } = string.Empty;
        public int? PhaseTypeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}