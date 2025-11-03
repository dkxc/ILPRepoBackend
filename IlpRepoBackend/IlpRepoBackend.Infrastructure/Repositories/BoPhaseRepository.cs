using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using IlpRepoBackend.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace IlpRepoBackend.Infrastructure.Repositories
{
    public class BoPhaseRepository : GenericRepository<BoPhase>, IBoPhaseRepository
    {
        private readonly AppDbContext _context;

        public BoPhaseRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public override async Task<BoPhase?> GetByIdAsync(int id)
        {
            return await _context.BoPhases
                .Include(bp => bp.Trainee)
                    .ThenInclude(t => t.User)
                .Include(bp => bp.Buddy)
                    .ThenInclude(b => b.Du)
                .FirstOrDefaultAsync(bp => bp.Id == id);
        }

        public async Task<IEnumerable<BoPhase>> GetByTraineeIdAsync(int traineeId)
        {
            return await _context.BoPhases
                .Include(bp => bp.Trainee)
                    .ThenInclude(t => t.User)
                .Include(bp => bp.Buddy)
                    .ThenInclude(b => b.Du)
                .Where(bp => bp.TraineeId == traineeId)
                .ToListAsync();
        }

        public async Task<IEnumerable<BoPhase>> GetByIdsAsync(IEnumerable<int> ids)
        {
            return await _context.BoPhases
                .Include(bp => bp.Trainee)
                    .ThenInclude(t => t.User)
                .Include(bp => bp.Buddy)
                    .ThenInclude(b => b.Du)
                .Where(bp => ids.Contains(bp.Id))
                .ToListAsync();
        }

        public async Task<IEnumerable<BoPhase>> GetBoPhaseDetailsByTraineeIds(IEnumerable<int> traineeIds)
        {
            return await _context.BoPhases
                .Include(bp => bp.Trainee)
                    .ThenInclude(t => t.User)
                .Include(bp => bp.Buddy)
                    .ThenInclude(b => b.Du)
                .Where(bp => traineeIds.Contains(bp.TraineeId ?? 0))
                .ToListAsync();
        }
    }
}