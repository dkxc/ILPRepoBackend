using System.Collections.Generic;

namespace IlpRepoBackend.Application.Dto.Results
{
    public class TraineeDetailedResultsDto
    {
        public int TraineeId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public List<PhaseResultDto> Phases { get; set; } = new();
    }

    public class PhaseResultDto
    {
        public string PhaseName { get; set; }
        public int? Score { get; set; }
        public Dictionary<string, string> Feedback { get; set; } = new();
    }
}