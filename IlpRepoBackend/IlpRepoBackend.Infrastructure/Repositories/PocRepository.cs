using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using IlpRepoBackend.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Infrastructure.Repositories
{
    public class PocRepository : GenericRepository<Poc>, IPocRepository
    {
        private readonly AppDbContext _context;

        public PocRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Poc?> GetByEmailAsync(string email)
        {
            // FIXED: Changed from SingleAsync to FirstOrDefaultAsync
            return await _context.Pocs
                .FirstOrDefaultAsync(p => p.Email == email);
        }

        public async Task<Poc?> GetByNameAsync(string name)
        {
            // FIXED: Changed from SingleAsync to FirstOrDefaultAsync
            return await _context.Pocs
                .FirstOrDefaultAsync(p => p.Name == name);
        }

        public async Task<List<Poc>> GetPocsByProjectIdAsync(int projectId)
        {
            return await _context.PocsForProjects
                .Where(pfp => pfp.ProjectId == projectId)
                .Select(pfp => pfp.Poc)
                .ToListAsync();
        }

       
    }
}
