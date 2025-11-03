using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Dto
{
    public class BatchPhaseDto
    {
        public string PhaseType { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class BatchDetailDto
    {
        public int BatchId { get; set; }
        public string BatchName { get; set; } = string.Empty;
        public string BatchStatus { get; set; } = string.Empty;
        public string BatchType { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int NoOfTrainees { get; set; }
        public int DayOfBatch { get; set; }
        public List<BatchPhaseDto> Phases { get; set; } = new();
    }
}
