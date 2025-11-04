using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IlpRepoBackend.Application.Dto.AdminDashboard;
using IlpRepoBackend.Application.Query.AdminDashboard;
using IlpRepoBackend.Domain.Persistence;

namespace IlpRepoBackend.Application.Handler.AdminDashboard
{
    public class GetTrainingHoursReportQueryHandler : IRequestHandler<GetTrainingHoursReportQuery, TrainingHoursSummaryDto>
    {
        private readonly ITrainingScheduleRepository _trainingScheduleRepository;
        private readonly IBatchTypeRepository _batchTypeRepository;

        public GetTrainingHoursReportQueryHandler(
            ITrainingScheduleRepository trainingScheduleRepository,
            IBatchTypeRepository batchTypeRepository)
        {
            _trainingScheduleRepository = trainingScheduleRepository;
            _batchTypeRepository = batchTypeRepository;
        }

        public async Task<TrainingHoursSummaryDto> Handle(GetTrainingHoursReportQuery request, CancellationToken cancellationToken)
        {
            // Get filtered data
            var schedules = await _trainingScheduleRepository
                .GetByBatchTypeAndDateRangeAsync(request.BatchTypeId, request.StartDate, request.EndDate);

            // Group by batch and summarize hours
            var batchDetails = schedules
                .GroupBy(ts => new
                {
                    ts.BatchId,
                    ts.Batch.BatchName,
                    BatchTypeName = ts.Batch.BatchType.Name
                })
                .Select(g => new TrainingHoursReportDto
                {
                    BatchName = g.Key.BatchName,
                    BatchTypeName = g.Key.BatchTypeName,
                    TotalTrainingHours = g.Sum(x => x.Hours)
                })
                .ToList();

            var totalHours = batchDetails.Sum(b => b.TotalTrainingHours);

            string batchTypeName = "All Batch Types";
            if (request.BatchTypeId.HasValue)
            {
                var batchType = await _batchTypeRepository.GetByIdAsync(request.BatchTypeId.Value);
                batchTypeName = batchType?.Name ?? "Unknown";
            }

            return new TrainingHoursSummaryDto
            {
                BatchTypeName = batchTypeName,
                TotalHours = totalHours,
                BatchDetails = batchDetails
            };
        }
    }
}
