using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using IlpRepoBackend.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IlpRepoBackend.Infrastructure.Repositories
{
    public class PocForAProjectRepository : GenericRepository<PocsForProject>, IPocForAProjectRepository
    {
        private readonly AppDbContext _context;

        public PocForAProjectRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PocsForProject>> GetByProjectIdAsync(int projectId)
        {
            return await _context.PocsForProjects
                .Where(p => p.ProjectId == projectId)
                .ToListAsync();
        }

        public async Task DeleteByProjectIdAndPocIdAsync(int projectId, int pocId)
        {
            var entity = await _context.PocsForProjects
                .FirstOrDefaultAsync(p => p.ProjectId == projectId && p.PocId == pocId);

            if (entity != null)
            {
                _context.PocsForProjects.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAllByProjectIdAsync(int projectId)
        {
            var entities = await _context.PocsForProjects
                .Where(p => p.ProjectId == projectId)
                .ToListAsync();

            if (entities.Any())
            {
                _context.PocsForProjects.RemoveRange(entities);
                await _context.SaveChangesAsync();
            }
        }
    }
}