using IlpRepoBackend.Domain.Enum;

namespace IlpRepoBackend.Domain.Entities
{
    public class Attendance
    {
        public int Id { get; set; }
        public int TraineeId { get; set; }
        public DateOnly Date { get; set; }
        public AttendanceStatus ForenoonStatus { get; set; } = AttendanceStatus.NA;
        public AttendanceStatus AfternoonStatus { get; set; } = AttendanceStatus.NA;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Trainee Trainee { get; set; } = null!;
    }
}
