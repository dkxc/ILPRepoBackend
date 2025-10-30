using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using IlpRepoBackend.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IlpRepoBackend.Infrastructure.Repositories
{
    public class MentorForPRojectRepository : GenericRepository<MenterForAProject>, IMentorForPRojectRepository
    {
        private readonly AppDbContext _context;

        public MentorForPRojectRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MenterForAProject>> GetByProjectIdAsync(int projectId)
        {
            return await _context.MentersForProjects
                .Where(m => m.ProjectId == projectId)
                .ToListAsync();
        }

        public async Task DeleteByProjectIdAndMentorIdAsync(int projectId, int mentorId)
        {
            var entity = await _context.MentersForProjects
                .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.MenterId == mentorId);

            if (entity != null)
            {
                _context.MentersForProjects.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAllByProjectIdAsync(int projectId)
        {
            var entities = await _context.MentersForProjects
                .Where(m => m.ProjectId == projectId)
                .ToListAsync();

            if (entities.Any())
            {
                _context.MentersForProjects.RemoveRange(entities);
                await _context.SaveChangesAsync();
            }
        }
    }
}