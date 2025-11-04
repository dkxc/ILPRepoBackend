using IlpRepoBackend.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IlpRepoBackend.Domain.Persistence
{
    public interface IAttendanceRepository : IGenericRepository<Attendance>
    {
        Task<List<Attendance>> GetByTraineeIdsAndDateRange(List<int> traineeIds, DateOnly startDate, DateOnly endDate);
        Task UpsertAttendanceRange(List<Attendance> attendanceRecords);
    }
}
