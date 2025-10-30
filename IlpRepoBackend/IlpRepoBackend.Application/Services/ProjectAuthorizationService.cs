using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;

namespace IlpRepoBackend.Application.Services
{
    public interface IProjectAuthorizationService
    {
        Task<bool> IsTeamLeaderAsync(int traineeId, int projectId);
        Task<bool> IsInProjectAsync(int traineeId, int projectId);
    }

    public class ProjectAuthorizationService : IProjectAuthorizationService
    {
        private readonly IProjectRepository _projectRepository;

        public ProjectAuthorizationService(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task<bool> IsTeamLeaderAsync(int traineeId, int projectId)
        {
            var projectTeam = await _projectRepository.GetProjectTeamMemberAsync(projectId, traineeId);
            
            if (projectTeam == null)
                return false;

            // Check if the trainee has TeamLead role
            return projectTeam.Role == Domain.Enum.ProjectRole.TeamLead;
        }

        public async Task<bool> IsInProjectAsync(int traineeId, int projectId)
        {
            var projectTeam = await _projectRepository.GetProjectTeamMemberAsync(projectId, traineeId);
            return projectTeam != null;
        }
    }
}