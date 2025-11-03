using System;

namespace IlpRepoBackend.Application.Dto
{
    public class BoPhaseDto
    {
        public int Id { get; set; }
        public int? TraineeId { get; set; }
        public string? TraineeName { get; set; }
        public int? BuddyId { get; set; }
        public string? BuddyName { get; set; }
        public string? DuName { get; set; }  // Added DU name
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}