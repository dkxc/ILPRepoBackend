using IlpRepoBackend.Application.Dto.Attendance;
using IlpRepoBackend.Application.Wrapper;
using MediatR;

namespace IlpRepoBackend.Application.Command.Attendance
{
    public class UpdateBatchAttendanceCommand : IRequest<ApiResponse<int>>
    {
        public int BatchId { get; set; }
        public UpdateBatchAttendanceDto UpdateData { get; set; } = new();
    }
}
