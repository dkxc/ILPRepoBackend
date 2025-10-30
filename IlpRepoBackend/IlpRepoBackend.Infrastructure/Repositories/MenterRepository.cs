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
    public class MenterRepository : GenericRepository<Mentor>, IMentorRepository
    {
        private readonly AppDbContext _context;

        public MenterRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Mentor?> GetByEmailAsync(string email)
        {
            // FIXED: Changed from SingleAsync to FirstOrDefaultAsync
            return await _context.Mentors
                .FirstOrDefaultAsync(m => m.Email == email);
        }

        public async Task<Mentor?> GetByNameAsync(string name)
        {
            // FIXED: Changed from SingleAsync to FirstOrDefaultAsync
            return await _context.Mentors
                .FirstOrDefaultAsync(m => m.Name == name);
        }

        public Task<IEnumerable<Mentor>> GetByProjectIdAsync(int projectId)
        {
            throw new NotImplementedException();
        }

        public Task<Mentor> UpdateAsync(Mentor entity)
        {
            throw new NotImplementedException();
        }
    }
}