using System;

namespace IlpRepoBackend.Application.Dto
{
    public class TraineeDuDto
    {
        public int Id { get; set; }
        public int? TraineeId { get; set; }
        public int? DuId { get; set; }
        public string Location { get; set; }
        public string OjtMenter { get; set; }
        public string TraineeName { get; set; }
        public string DuName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}