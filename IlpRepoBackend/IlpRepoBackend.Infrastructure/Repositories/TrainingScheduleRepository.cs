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

        public async Task<IEnumerable<TrainingSchedule>> GetByBatchIdAsync(int batchId)
        {
            return await _context.TrainingSchedules
                .Where(t => t.BatchId == batchId)
                .ToListAsync();
        }

    }
}
