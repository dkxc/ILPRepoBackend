using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using IlpRepoBackend.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace IlpRepoBackend.Infrastructure.Repositories
{
    public class DuRepository : GenericRepository<Du>, IDuRepository
    {
        private readonly AppDbContext _context;

        public DuRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Du?> GetByNameAsync(string name)
        {
            return await _context.Dus
                .FirstOrDefaultAsync(d => d.Name == name);
        }

        public override async Task<Du?> GetByIdAsync(int id)
        {
            return await _context.Dus
                .Include(d => d.Buddies)
                .Include(d => d.TraineeDus)
                .FirstOrDefaultAsync(d => d.Id == id);
        }
    }
}