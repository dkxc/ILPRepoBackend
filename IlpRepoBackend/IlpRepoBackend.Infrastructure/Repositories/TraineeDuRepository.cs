using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using IlpRepoBackend.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IlpRepoBackend.Infrastructure.Repositories
{
    public class TraineeDuRepository : GenericRepository<TraineeDu>, ITraineeDuRepository
    {
        private readonly AppDbContext _context;

        public TraineeDuRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TraineeDu>> GetByIdsAsync(IEnumerable<int> ids)
        {
            return await _context.TraineeDus
                .Include(td => td.Trainee)
                    .ThenInclude(t => t.User)
                .Include(td => td.Du)
                .Where(td => ids.Contains(td.Id))
                .ToListAsync();
        }

        public async Task<IEnumerable<TraineeDu>> GetByTraineeIdAsync(int traineeId)
        {
            return await _context.TraineeDus
                .Include(td => td.Trainee)
                    .ThenInclude(t => t.User)
                .Include(td => td.Du)
                .Where(td => td.TraineeId == traineeId)
                .ToListAsync();
        }

        public async Task<IEnumerable<TraineeDu>> GetTraineeDuDetailsByTraineeIds(IEnumerable<int> traineeIds)
        {
            return await _context.TraineeDus
                .Include(td => td.Trainee)
                    .ThenInclude(t => t.User)
                .Include(td => td.Du)
                .Where(td => traineeIds.Contains(td.TraineeId ?? 0))
                .ToListAsync();
        }

        public override async Task<TraineeDu?> GetByIdAsync(int id)
        {
            return await _context.TraineeDus
                .Include(td => td.Trainee)
                    .ThenInclude(t => t.User)
                .Include(td => td.Du)
                .FirstOrDefaultAsync(td => td.Id == id);
        }
    }
}