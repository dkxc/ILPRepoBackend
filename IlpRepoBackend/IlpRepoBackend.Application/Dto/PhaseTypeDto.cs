using System;

namespace IlpRepoBackend.Application.Dto
{
    public class PhaseTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}