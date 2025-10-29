using IlpRepoBackend.Domain.Entities;
using System.Threading.Tasks;

namespace IlpRepoBackend.Domain.Persistence
{
    public interface IBatchRepository : IGenericRepository<Batch>
    {
        Task<Batch?> GetByIdAsync(int id);
    }
}
