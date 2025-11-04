using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;

using IlpRepoBackend.Application.Dto;
using System.Text;
using System.Threading.Tasks;
using IlpRepoBackend.Application.Dto.AdminDashboard;

namespace IlpRepoBackend.Application.Query.AdminDashboard
{
    public class GetTrainingHoursReportQuery : IRequest<TrainingHoursSummaryDto>
    {
        public int? BatchTypeId { get; set; }   // null => all types
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public GetTrainingHoursReportQuery(int? batchTypeId, DateTime startDate, DateTime endDate)
        {
            BatchTypeId = batchTypeId;
            StartDate = startDate;
            EndDate = endDate;
        }
    }
}
