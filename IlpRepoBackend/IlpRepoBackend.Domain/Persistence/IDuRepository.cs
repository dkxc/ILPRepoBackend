using IlpRepoBackend.Domain.Entities;
using System.Threading.Tasks;

namespace IlpRepoBackend.Domain.Persistence
{
    public interface IDuRepository : IGenericRepository<Du>
    {
        Task<Du?> GetByNameAsync(string name);
    }
}
