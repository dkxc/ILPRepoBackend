<<<<<<< HEAD
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using IlpRepoBackend.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
=======
﻿using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using IlpRepoBackend.Infrastructure.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
>>>>>>> b1a77052b2965f66ef4845525863870bf757b159

namespace IlpRepoBackend.Infrastructure.Repositories
{
    public class ProjectRepository : GenericRepository<Project>, IProjectRepository
    {
<<<<<<< HEAD
        public ProjectRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Project?> GetProjectDetailsAsync(int projectId)
        {
            return await GetProjectWithDetailsAsync(projectId);
        }

        public async Task<Project?> GetProjectWithLinksAsync(int projectId)
        {
            return await _context.Projects
                .Include(p => p.ProjectLinks)
                    .ThenInclude(pl => pl.Link)
                .FirstOrDefaultAsync(p => p.Id == projectId);
        }

        public async Task<Project?> GetProjectWithDetailsAsync(int projectId)
        {
            return await _context.Projects
                .Include(p => p.ProjectTeams)
                    .ThenInclude(pt => pt.Trainee)
                        .ThenInclude(t => t.User)
                .Include(p => p.ProjectTeams)
                    .ThenInclude(pt => pt.Trainee)
                        .ThenInclude(t => t.Batch)
                .Include(p => p.ProjectLinks)
                    .ThenInclude(pl => pl.Link)
                .Include(p => p.Mentors)
                .Include(p => p.Pocs)
                .FirstOrDefaultAsync(p => p.Id == projectId);
        }

        public async Task<IEnumerable<ProjectLink>> GetProjectLinksAsync(int projectId)
        {
            return await _context.ProjectLinks
                .Where(pl => pl.ProjectId == projectId)
                .ToListAsync();
        }

        public async Task AddProjectLinksAsync(List<ProjectLink> projectLinks)
        {
            await _context.ProjectLinks.AddRangeAsync(projectLinks);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveProjectLinksAsync(List<ProjectLink> projectLinks)
        {
            _context.ProjectLinks.RemoveRange(projectLinks);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateProjectAsync(Project project)
        {
            _context.Projects.Update(project);
            await _context.SaveChangesAsync();
        }

        public async Task<ProjectTeam?> GetProjectTeammateAsync(int projectId, int traineeId)
        {
            return await GetProjectTeamMemberAsync(projectId, traineeId);
        }

        public async Task<ProjectTeam?> GetProjectTeamMemberAsync(int projectId, int traineeId)
        {
            return await _context.ProjectTeams
                .Include(pt => pt.Trainee)
                .FirstOrDefaultAsync(pt => pt.ProjectId == projectId && pt.TraineeId == traineeId);
        }

        public async Task RemoveTeammateAsync(ProjectTeam projectTeam)
        {
            _context.ProjectTeams.Remove(projectTeam);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsTraineeInProjectAsync(int projectId, int traineeId)
        {
            return await _context.ProjectTeams
                .AnyAsync(pt => pt.ProjectId == projectId && pt.TraineeId == traineeId);
=======
        private readonly AppDbContext _context;
        public ProjectRepository(AppDbContext context) : base(context)
        {
            _context = context;
>>>>>>> b1a77052b2965f66ef4845525863870bf757b159
        }
    }
}
