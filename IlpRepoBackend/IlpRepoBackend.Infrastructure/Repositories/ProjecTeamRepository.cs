using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using IlpRepoBackend.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IlpRepoBackend.Infrastructure.Repositories
{
    public class ProjecTeamRepository : GenericRepository<ProjectTeam>, IProjecTeamRepository
    {
        private readonly AppDbContext _context;

        public ProjecTeamRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProjectTeam>> GetByProjectIdAsync(int projectId)
        {
            return await _context.ProjectTeams
                .Where(pt => pt.ProjectId == projectId)
                .ToListAsync();
        }

        public async Task DeleteByProjectIdAndTraineeIdAsync(int projectId, int traineeId)
        {
            var entity = await _context.ProjectTeams
                .FirstOrDefaultAsync(pt => pt.ProjectId == projectId && pt.TraineeId == traineeId);

            if (entity != null)
            {
                _context.ProjectTeams.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAllByProjectIdAsync(int projectId)
        {
            var entities = await _context.ProjectTeams
                .Where(pt => pt.ProjectId == projectId)
                .ToListAsync();

            if (entities.Any())
            {
                _context.ProjectTeams.RemoveRange(entities);
                await _context.SaveChangesAsync();
            }
        }
    }
}