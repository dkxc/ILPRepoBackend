using IlpRepoBackend.Domain.Enum;
using System;
using System.Collections.Generic;

namespace IlpRepoBackend.Application.Dto
{
    public class BatchDto
    {
        public int Id { get; set; }
        public string BatchName { get; set; } = string.Empty;
        public int? BatchTypeId { get; set; }
        public string? BatchTypeName { get; set; }
        public BatchStatus Status { get; set; } = BatchStatus.NotStarted;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<PhaseDto> Phases { get; set; } = new List<PhaseDto>();
    }
}
