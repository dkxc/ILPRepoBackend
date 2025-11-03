using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using IlpRepoBackend.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace IlpRepoBackend.Infrastructure.Repositories
{
    public class BuddyRepository : GenericRepository<Buddy>, IBuddyRepository
    {
        private readonly AppDbContext _context;

        public BuddyRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Buddy>> GetByDuIdAsync(int duId)
        {
            return await _context.Buddies
                .Where(b => b.DuId == duId)
                .ToListAsync();
        }

        public async Task<Buddy?> GetByNameAsync(string name)
        {
            return await _context.Buddies
                .FirstOrDefaultAsync(b => b.Name == name);
        }

        public override async Task<Buddy?> GetByIdAsync(int id)
        {
            return await _context.Buddies
                .Include(b => b.Du)
                .Include(b => b.BoPhases)
                .FirstOrDefaultAsync(b => b.Id == id);
        }
    }
}