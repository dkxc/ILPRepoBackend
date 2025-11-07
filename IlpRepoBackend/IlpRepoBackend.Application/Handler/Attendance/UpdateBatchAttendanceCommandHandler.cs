using IlpRepoBackend.Application.Command.Attendance;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace IlpRepoBackend.Application.Handler.Attendance
{
    public class UpdateBatchAttendanceCommandHandler : IRequestHandler<UpdateBatchAttendanceCommand, ApiResponse<int>>
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly ITraineeRepository _traineeRepository;

        public UpdateBatchAttendanceCommandHandler(IAttendanceRepository attendanceRepository, ITraineeRepository traineeRepository)
        {
            _attendanceRepository = attendanceRepository;
            _traineeRepository = traineeRepository;
        }

        public async Task<ApiResponse<int>> Handle(UpdateBatchAttendanceCommand request, CancellationToken cancellationToken)
        {
            var updateData = request.UpdateData;
            if (updateData.TraineeIds == null || !updateData.TraineeIds.Any())
                return ApiResponse<int>.Fail("At least one trainee must be selected.");

            var startDate = DateOnly.Parse(updateData.StartDate, CultureInfo.InvariantCulture);
            var endDate = DateOnly.Parse(updateData.EndDate, CultureInfo.InvariantCulture);

            var existingRecords = await _attendanceRepository.GetByTraineeIdsAndDateRange(updateData.TraineeIds, startDate, endDate);
            var recordsToUpsert = new List<Domain.Entities.Attendance>();

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                foreach (var traineeId in updateData.TraineeIds)
                {
                    var existing = existingRecords.FirstOrDefault(r => r.TraineeId == traineeId && r.Date == date);
                    if (existing != null)
                    {
                        if (updateData.Status.Forenoon.HasValue) existing.ForenoonStatus = updateData.Status.Forenoon.Value;
                        if (updateData.Status.Afternoon.HasValue) existing.AfternoonStatus = updateData.Status.Afternoon.Value;
                        recordsToUpsert.Add(existing);
                    }
                    else
                    {
                        recordsToUpsert.Add(new Domain.Entities.Attendance
                        {
                            TraineeId = traineeId,
                            Date = date,
                            ForenoonStatus = updateData.Status.Forenoon ?? Domain.Enum.AttendanceStatus.P,
                            AfternoonStatus = updateData.Status.Afternoon ?? Domain.Enum.AttendanceStatus.P
                        });
                    }
                }
            }

            await _attendanceRepository.UpsertAttendanceRange(recordsToUpsert);

            return ApiResponse<int>.Success(recordsToUpsert.Count, $"{recordsToUpsert.Count} records updated.");
        }
    }
}
