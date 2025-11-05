using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using IlpRepoBackend.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Infrastructure.Repositories
{
    public class TrainingScheduleRepository : GenericRepository<TrainingSchedule>, ITrainingScheduleRepository
    {
        private readonly AppDbContext _context;
        public TrainingScheduleRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TrainingSchedule>> GetByBatchTypeAndDateRangeAsync(int? batchTypeId, DateTime startDate, DateTime endDate)
        {
            var query = _context.TrainingSchedules
                .Include(ts => ts.Batch)
                .ThenInclude(b => b.BatchType)
                .Where(ts => ts.TrainingDate >= startDate && ts.TrainingDate <= endDate);

            if (batchTypeId.HasValue)
                query = query.Where(ts => ts.Batch.BatchTypeId == batchTypeId.Value);

            return await query.ToListAsync();
        }
        public async Task<IEnumerable<TrainingSchedule>> GetByBatchIdAsync(int batchId)
        {
            return await _context.TrainingSchedules
                .Where(t => t.BatchId == batchId)
                .ToListAsync();
        }
        public async Task AddRangeAsync(IEnumerable<TrainingSchedule> schedules)
        {
            await _context.TrainingSchedules.AddRangeAsync(schedules);
            await _context.SaveChangesAsync();
        }
        public async Task<TrainingSchedule?> GetByBatchAndDateAsync(int batchId, DateTime date)
        {
            return await _context.TrainingSchedules
                .FirstOrDefaultAsync(t =>
                    t.BatchId == batchId &&
                    t.TrainingDate.Date == date.Date);
        }

        public async Task UpdateOrCreateAsync(TrainingSchedule schedule)
        {
            if (schedule.Id == 0)
                _context.TrainingSchedules.Add(schedule);
            else
                _context.TrainingSchedules.Update(schedule);

            await _context.SaveChangesAsync();
        }

    }
}
