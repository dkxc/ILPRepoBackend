using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;

public interface IProjectRepository : IGenericRepository<Project>
{
    Task AddTeamMemberAsync(ProjectTeam projectTeam);
    Task<IEnumerable<Project>> GetAllProjectsWithDetailsAsync(); // Fixed return type
    Task GetByIdWithDetailsAsync(int id);
    Task<Project?> GetProjectWithDetailsAsync(int projectId);
    Task<IEnumerable<Project>> GetProjectsByBatchIdAsync(int batchId);
}