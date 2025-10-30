using IlpRepoBackend.Domain.Entities;

namespace IlpRepoBackend.Domain.Persistence
{
    public interface IProjectRepository : IGenericRepository<Project>
    {
        Task<Project?> GetProjectDetailsAsync(int projectId);
        Task<Project?> GetProjectWithLinksAsync(int projectId);
        Task AddProjectLinksAsync(List<ProjectLink> projectLinks);
        Task RemoveProjectLinksAsync(List<ProjectLink> projectLinks);
        Task UpdateProjectAsync(Project project);
        Task<ProjectTeam?> GetProjectTeammateAsync(int projectId, int traineeId);
        Task RemoveTeammateAsync(ProjectTeam projectTeam);
        Task<bool> IsTraineeInProjectAsync(int projectId, int traineeId);
        Task<ProjectTeam?> GetProjectTeamMemberAsync(int projectId, int traineeId);
    }
}
