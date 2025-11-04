using IlpRepoBackend.Application.Dto.Attendance;
using IlpRepoBackend.Application.Wrapper;
using MediatR;
using System.Collections.Generic;

namespace IlpRepoBackend.Application.Command.Attendance
{
    public class UploadBatchAttendanceCommand : IRequest<ApiResponse<string>>
    {
        public int BatchId { get; set; }
        public List<UploadAttendanceDto> UploadData { get; set; } = new();
    }
}
