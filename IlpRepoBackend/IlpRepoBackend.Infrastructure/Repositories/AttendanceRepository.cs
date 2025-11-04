using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using IlpRepoBackend.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IlpRepoBackend.Infrastructure.Repositories
{
    public class AttendanceRepository : GenericRepository<Attendance>, IAttendanceRepository
    {
        public AttendanceRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<Attendance>> GetByTraineeIdsAndDateRange(List<int> traineeIds, DateOnly startDate, DateOnly endDate)
        {
            return await _dbSet
                .Where(a => traineeIds.Contains(a.TraineeId) && a.Date >= startDate && a.Date <= endDate)
                .ToListAsync();
        }

        public async Task UpsertAttendanceRange(List<Attendance> attendanceRecords)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                var traineeIds = attendanceRecords.Select(r => r.TraineeId).Distinct();
                var dates = attendanceRecords.Select(r => r.Date).Distinct();

                var existingRecords = await _dbSet
                    .Where(a => traineeIds.Contains(a.TraineeId) && dates.Contains(a.Date))
                    .ToListAsync();

                var existingRecordsDict = existingRecords.ToDictionary(a => (a.TraineeId, a.Date));

                var recordsToAdd = new List<Attendance>();

                foreach (var newRecord in attendanceRecords)
                {
                    if (existingRecordsDict.TryGetValue((newRecord.TraineeId, newRecord.Date), out var existingRecord))
                    {
                        // Update existing record
                        existingRecord.ForenoonStatus = newRecord.ForenoonStatus;
                        existingRecord.AfternoonStatus = newRecord.AfternoonStatus;
                        existingRecord.UpdatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        // Add new record
                        newRecord.CreatedAt = DateTime.UtcNow;
                        newRecord.UpdatedAt = DateTime.UtcNow;
                        recordsToAdd.Add(newRecord);
                    }
                }

                if (recordsToAdd.Any())
                {
                    await _dbSet.AddRangeAsync(recordsToAdd);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            });
        }
    }
}
