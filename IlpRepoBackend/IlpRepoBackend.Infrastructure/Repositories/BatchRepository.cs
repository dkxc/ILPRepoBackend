using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using IlpRepoBackend.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace IlpRepoBackend.Infrastructure.Repositories
{
    public class BatchRepository : GenericRepository<Batch>, IBatchRepository
    {
        private readonly AppDbContext _context;

        public BatchRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Batch?> GetByIdAsync(int id)
        {
            return await _context.Batches.FirstOrDefaultAsync(u => u.Id == id);
        }
    }
}
