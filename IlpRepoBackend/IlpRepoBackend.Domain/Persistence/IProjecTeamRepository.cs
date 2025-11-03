using IlpRepoBackend.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IlpRepoBackend.Domain.Persistence
{
    public interface IProjecTeamRepository : IGenericRepository<ProjectTeam>
    {
        Task<IEnumerable<ProjectTeam>> GetByProjectIdAsync(int projectId);
        Task DeleteByProjectIdAndTraineeIdAsync(int projectId, int traineeId);
        Task DeleteAllByProjectIdAsync(int projectId);
    }
}