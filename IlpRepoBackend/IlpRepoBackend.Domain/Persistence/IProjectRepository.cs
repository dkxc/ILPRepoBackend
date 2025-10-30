<<<<<<< HEAD
using IlpRepoBackend.Domain.Entities;
=======
﻿using IlpRepoBackend.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
>>>>>>> b1a77052b2965f66ef4845525863870bf757b159

namespace IlpRepoBackend.Domain.Persistence
{
    public interface IProjectRepository : IGenericRepository<Project>
    {
<<<<<<< HEAD
        Task<Project?> GetProjectDetailsAsync(int projectId);
        Task<Project?> GetProjectWithLinksAsync(int projectId);
        Task AddProjectLinksAsync(List<ProjectLink> projectLinks);
        Task RemoveProjectLinksAsync(List<ProjectLink> projectLinks);
        Task UpdateProjectAsync(Project project);
        Task<ProjectTeam?> GetProjectTeammateAsync(int projectId, int traineeId);
        Task RemoveTeammateAsync(ProjectTeam projectTeam);
        Task<bool> IsTraineeInProjectAsync(int projectId, int traineeId);
        Task<ProjectTeam?> GetProjectTeamMemberAsync(int projectId, int traineeId);
=======
>>>>>>> b1a77052b2965f66ef4845525863870bf757b159
    }
}
