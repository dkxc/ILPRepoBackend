using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using IlpRepoBackend.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace IlpRepoBackend.Infrastructure.Repositories
{
    public class LinkRepository : GenericRepository<Link>, ILinkRepository
    {
        public LinkRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Link>> GetAllLinkTypesAsync()
        {
            return await _context.Links
                .OrderBy(l => l.Name)
                .ToListAsync();
        }
    }
}