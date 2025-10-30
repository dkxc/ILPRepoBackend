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
    public class TraineeRepository : GenericRepository<Trainee>, ITraineeRepository
    {
        private readonly AppDbContext _context;
        public TraineeRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Trainee>> GetByBatchIdAsync(int batchId)
        {
            return await _context.Trainees.Where(t => t.BatchId == batchId).ToListAsync();
        }

        public Task<object> GetTraineesByBatchId(object batchId)
        {
            return Task.FromResult((object)_context.Trainees.Where(t => t.BatchId == (int)batchId).ToList());
        }
    }
}
