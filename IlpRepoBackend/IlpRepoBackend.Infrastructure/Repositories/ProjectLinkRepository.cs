using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using IlpRepoBackend.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IlpRepoBackend.Infrastructure.Repositories
{
    public class ProjectLinkRepository : GenericRepository<ProjectLink>, IProjectLinkRepository
    {
        private readonly AppDbContext _context;

        public ProjectLinkRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProjectLink>> GetByProjectIdAsync(int projectId)
        {
            return await _context.ProjectLinks
                .Include(pl => pl.Link)
                .Where(pl => pl.ProjectId == projectId)
                .ToListAsync();
        }

        public async Task DeleteByProjectIdAsync(int projectId)
        {
            var projectLinks = await _context.ProjectLinks
                .Where(pl => pl.ProjectId == projectId)
                .ToListAsync();

            if (projectLinks.Any())
            {
                _context.ProjectLinks.RemoveRange(projectLinks);
                await _context.SaveChangesAsync();
            }
        }
    }
}