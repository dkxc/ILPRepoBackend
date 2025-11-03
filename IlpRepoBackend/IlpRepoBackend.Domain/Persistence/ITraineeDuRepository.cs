using IlpRepoBackend.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IlpRepoBackend.Domain.Persistence
{
    public interface ITraineeDuRepository : IGenericRepository<TraineeDu>
    {
        Task<IEnumerable<TraineeDu>> GetByTraineeIdAsync(int traineeId);
        Task<IEnumerable<TraineeDu>> GetByIdsAsync(IEnumerable<int> ids);
        Task<IEnumerable<TraineeDu>> GetTraineeDuDetailsByTraineeIds(IEnumerable<int> traineeIds);
    }
}
