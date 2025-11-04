using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using IlpRepoBackend.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IlpRepoBackend.Infrastructure.Repositories
{
    public class CurriculumRepository : GenericRepository<Curriculum>, ICurriculumRepository
    {
        private readonly AppDbContext _context;

        public CurriculumRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Curriculum>> GetByBatchIdAsync(int batchId)
        {
            return await _context.Curriculums
                                 .Where(c => c.BatchId == batchId)
                                 .ToListAsync();
        }
    }
}
