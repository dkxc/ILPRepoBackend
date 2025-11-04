using IlpRepoBackend.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IlpRepoBackend.Domain.Persistence
{
    public interface ICurriculumRepository : IGenericRepository<Curriculum>
    {
        Task<List<Curriculum>> GetByBatchIdAsync(int batchId);
    }
}
