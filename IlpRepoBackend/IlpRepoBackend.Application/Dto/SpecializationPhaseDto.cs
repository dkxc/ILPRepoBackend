namespace IlpRepoBackend.Application.Dto
{
    public class SpecializationPhaseDto
    {
        public int Id { get; set; }
        public int TraineeId { get; set; }
        public string TraineeName { get; set; }
        public string TechStack { get; set; }
        public string Project { get; set; }
        public int? SpecializationPhaseId { get; set; }
    }
}