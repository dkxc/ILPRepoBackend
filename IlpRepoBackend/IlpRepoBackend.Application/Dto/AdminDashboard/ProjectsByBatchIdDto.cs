
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Dto
{
    public class ProjectsByBatchIdDto
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string TeamLeadName { get; set; } = string.Empty;
        public int NoOfTrainees { get; set; }
        public string Technology { get; set; } = string.Empty;
        public double SubmissionRate { get; set; }  // Placeholder for now
        public string Status { get; set; } = string.Empty;
    }
}
