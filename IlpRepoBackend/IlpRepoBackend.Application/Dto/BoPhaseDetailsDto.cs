using System;

namespace IlpRepoBackend.Application.Dto
{
    public class BoPhaseDetailsDto
    {
        public int BoPhaseId { get; set; }
        public string? TraineeName { get; set; }
        public string? Buddy { get; set; }
        public string? BuddyDU { get; set; }
    }
}