using IlpRepoBackend.Application.Dto.Attendance;
using IlpRepoBackend.Application.Query.Attendance;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Enum;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System.Globalization;

namespace IlpRepoBackend.Application.Handler.Attendance
{
    public class GetAttendanceByBatchQueryHandler : IRequestHandler<GetAttendanceByBatchQuery, ApiResponse<List<BatchAttendanceResponseDto>>>
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly ITraineeRepository _traineeRepository;

        public GetAttendanceByBatchQueryHandler(IAttendanceRepository attendanceRepository, ITraineeRepository traineeRepository)
        {
            _attendanceRepository = attendanceRepository;
            _traineeRepository = traineeRepository;
        }

        public async Task<ApiResponse<List<BatchAttendanceResponseDto>>> Handle(GetAttendanceByBatchQuery request, CancellationToken cancellationToken)
        {
            var trainees = (await _traineeRepository.GetByBatchIdAsync(request.BatchId)).ToList();
            if (!trainees.Any())
            {
                return ApiResponse<List<BatchAttendanceResponseDto>>.Success(new List<BatchAttendanceResponseDto>(), "No trainees found in this batch.");
            }

            var traineeIds = trainees.Select(t => t.Id).ToList();

            // Set default date range if not provided
            var endDate = request.EndDate ?? DateOnly.FromDateTime(DateTime.Today);
            var startDate = request.StartDate ?? endDate.AddDays(-6); // Default to last 7 days

            var attendanceExceptions = await _attendanceRepository.GetByTraineeIdsAndDateRange(traineeIds, startDate, endDate);
            var exceptionsMap = attendanceExceptions
                .GroupBy(a => a.TraineeId)
                .ToDictionary(
                    g => g.Key,
                    g => g.ToDictionary(rec => rec.Date)
            );

            var response = new List<BatchAttendanceResponseDto>();

            foreach (var trainee in trainees)
            {
                var traineeAttendance = new BatchAttendanceResponseDto
                {
                    TraineeId = trainee.Id,
                    TraineeName = trainee.User.Username
                };

                for (var date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    var dateString = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                    if (exceptionsMap.TryGetValue(trainee.Id, out var traineeRecords) && traineeRecords.TryGetValue(date, out var record))
                    {
                        // An exception was found directly.
                        traineeAttendance.Dates[dateString] = new DailyAttendanceDto
                        {
                            Forenoon = record.ForenoonStatus,
                            Afternoon = record.AfternoonStatus
                        };
                    }
                    else
                    {
                        // No exception found, default to Present.
                        traineeAttendance.Dates[dateString] = new DailyAttendanceDto
                        {
                            Forenoon = AttendanceStatus.P,
                            Afternoon = AttendanceStatus.P
                        };
                    }
                    // --- END SINGLE LOOKUP ---
                }
                response.Add(traineeAttendance);
            }

            return ApiResponse<List<BatchAttendanceResponseDto>>.Success(response);
        }
    }
}
