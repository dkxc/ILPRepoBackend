using IlpRepoBackend.Application.Command.Attendance;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handler.Attendance
{
    public class UploadBatchAttendanceCommandHandler : IRequestHandler<UploadBatchAttendanceCommand, ApiResponse<string>>
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly ITraineeRepository _traineeRepository;

        public UploadBatchAttendanceCommandHandler(IAttendanceRepository attendanceRepository, ITraineeRepository traineeRepository)
        {
            _attendanceRepository = attendanceRepository;
            _traineeRepository = traineeRepository;
        }

        public async Task<ApiResponse<string>> Handle(UploadBatchAttendanceCommand request, CancellationToken cancellationToken)
        {
            var traineesInBatch = (await _traineeRepository.GetByBatchIdAsync(request.BatchId)).ToList();
            if (!traineesInBatch.Any())
                return ApiResponse<string>.Fail("No trainees found in the specified batch.");

            var traineeEmailMap = traineesInBatch.ToDictionary(
                t => t.User.Username,
                t => t.Id,
                StringComparer.OrdinalIgnoreCase // Use case-insensitive comparison for emails
            );

            var recordsToUpsert = new List<Domain.Entities.Attendance>();
            var invalidTrainees = new List<string>();

            foreach (var uploadItem in request.UploadData)
            {
                if (traineeEmailMap.TryGetValue(uploadItem.TraineeName, out var traineeId))
                {
                    recordsToUpsert.Add(new Domain.Entities.Attendance
                    {
                        TraineeId = traineeId,
                        Date = DateOnly.Parse(uploadItem.Date, CultureInfo.InvariantCulture),
                        ForenoonStatus = uploadItem.Forenoon,
                        AfternoonStatus = uploadItem.Afternoon
                    });
                }
                else
                {
                    invalidTrainees.Add(uploadItem.TraineeName);
                }
            }

            if (recordsToUpsert.Any())
            {
                await _attendanceRepository.UpsertAttendanceRange(recordsToUpsert);
            }

            if (invalidTrainees.Any())
            {
                return ApiResponse<string>.Fail($"Processed {recordsToUpsert.Count} records, but failed to find the following trainees in this batch: {string.Join(", ", invalidTrainees.Distinct())}");
            }

            return ApiResponse<string>.Success($"Successfully processed {recordsToUpsert.Count} attendance records.");
        }
    }
}
