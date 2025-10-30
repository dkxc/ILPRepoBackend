using IlpRepoBackend.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IlpRepoBackend.Domain.Persistence
{
    public interface IPocForAProjectRepository : IGenericRepository<PocsForProject>
    {
        Task<IEnumerable<PocsForProject>> GetByProjectIdAsync(int projectId);
        Task DeleteByProjectIdAndPocIdAsync(int projectId, int pocId);
        Task DeleteAllByProjectIdAsync(int projectId);
    }
}