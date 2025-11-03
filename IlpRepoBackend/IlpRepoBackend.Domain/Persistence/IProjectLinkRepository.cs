using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IlpRepoBackend.Domain.Persistence
{
    public interface IProjectLinkRepository : IGenericRepository<ProjectLink>
    {
        Task<IEnumerable<ProjectLink>> GetByProjectIdAsync(int projectId);
        Task DeleteByProjectIdAsync(int projectId);
    }
}