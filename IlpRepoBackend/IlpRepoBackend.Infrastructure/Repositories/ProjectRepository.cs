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
    public class ProjectRepository : GenericRepository<Project>, IProjectRepository
    {
        private readonly AppDbContext _context;

        public ProjectRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public Task AddTeamMemberAsync(ProjectTeam projectTeam)
        {
            throw new NotImplementedException();
        }

        // FIXED: Implemented GetAllProjectsWithDetailsAsync
        public async Task<IEnumerable<Project>> GetAllProjectsWithDetailsAsync()
        {
            return await _context.Projects
                .Include(p => p.ProjectTeams)
                    .ThenInclude(pt => pt.Trainee)
                        .ThenInclude(t => t.User)
                .Include(p => p.MentersForProjects)
                    .ThenInclude(m => m.Mentor)
                .Include(p => p.PocsForProjects)
                    .ThenInclude(p => p.Poc)
                .Include(p => p.DocumentRequests)
                    .ThenInclude(dr => dr.Document)
                .Include(p => p.DocumentRequests)
                    .ThenInclude(dr => dr.DocumentSubmissions)
                .ToListAsync();
        }

        public Task GetByIdWithDetailsAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<Project?> GetProjectWithDetailsAsync(int projectId)
        {
            return await _context.Projects
                .Include(p => p.ProjectTeams)
                    .ThenInclude(pt => pt.Trainee)
                        .ThenInclude(t => t.User)
                .Include(p => p.MentersForProjects)
                    .ThenInclude(m => m.Mentor)
                .Include(p => p.PocsForProjects)
                    .ThenInclude(p => p.Poc)
                .Include(p => p.ProjectLinks)
                    .ThenInclude(pl => pl.Link)
                .Include(p => p.DocumentRequests)
                    .ThenInclude(dr => dr.Document)
                .Include(p => p.DocumentRequests)
                    .ThenInclude(dr => dr.DocumentSubmissions)
                .FirstOrDefaultAsync(p => p.Id == projectId);
        }

        public async Task<IEnumerable<Project>> GetProjectsByBatchIdAsync(int batchId)
        {
            return await _context.Projects
                .Include(p => p.ProjectTeams)
                    .ThenInclude(pt => pt.Trainee)
                        .ThenInclude(t => t.User)
                .Include(p => p.ProjectLinks)
                    .ThenInclude(pl => pl.Link)
                .Where(p => p.ProjectTeams.Any(pt => pt.Trainee != null && pt.Trainee.BatchId == batchId))
                .Distinct()
                .ToListAsync();
        }
    }
}