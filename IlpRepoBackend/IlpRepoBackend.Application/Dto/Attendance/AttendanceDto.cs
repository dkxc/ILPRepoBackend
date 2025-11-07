using IlpRepoBackend.Domain.Enum;
using System.Collections.Generic;

namespace IlpRepoBackend.Application.Dto.Attendance
{
    // For GET response
    public class BatchAttendanceResponseDto
    {
        public int TraineeId { get; set; }
        public string TraineeName { get; set; } = string.Empty;
        public Dictionary<string, DailyAttendanceDto> Dates { get; set; } = new();
    }

    public class DailyAttendanceDto
    {
        public AttendanceStatus Forenoon { get; set; }
        public AttendanceStatus Afternoon { get; set; }
    }

    // For PUT request
    public class UpdateBatchAttendanceDto
    {
        public List<int> TraineeIds { get; set; } = new();
        public string StartDate { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;
        public UpdateStatusDto Status { get; set; } = new();
    }

    public class UpdateStatusDto
    {
        public AttendanceStatus? Forenoon { get; set; }
        public AttendanceStatus? Afternoon { get; set; }
    }

    // For POST (upload) request
    public class UploadAttendanceDto
    {
        public string TraineeName { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public AttendanceStatus Forenoon { get; set; }
        public AttendanceStatus Afternoon { get; set; }
    }
}
