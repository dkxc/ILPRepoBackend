using IlpRepoBackend.Application.Dto.Attendance;
using IlpRepoBackend.Application.Wrapper;
using MediatR;

namespace IlpRepoBackend.Application.Query.Attendance
{
    public class GetAttendanceByBatchQuery : IRequest<ApiResponse<List<BatchAttendanceResponseDto>>>
    {
        public int BatchId { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
    }
}
