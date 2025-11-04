using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Dto.AdminDashboard
{
    public class TrainingHoursSummaryDto
    {
        public string BatchTypeName { get; set; } = string.Empty;
        public int TotalHours { get; set; }
        public List<TrainingHoursReportDto> BatchDetails { get; set; } = new();
    }

    public class TrainingHoursReportDto
    {
        public string BatchName { get; set; } = string.Empty;
        public string BatchTypeName { get; set; } = string.Empty;
        public int TotalTrainingHours { get; set; }
    }
}

