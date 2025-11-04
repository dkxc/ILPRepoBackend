using System.Collections.Generic;

namespace IlpRepoBackend.Application.Dto.Results
{
    public class TraineeScoreDto
    {
        public int TraineeId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public Dictionary<string, int?> Scores { get; set; } = new();
    }
}