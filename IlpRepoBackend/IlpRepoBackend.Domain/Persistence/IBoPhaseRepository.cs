using IlpRepoBackend.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IlpRepoBackend.Domain.Persistence
{
    public interface IBoPhaseRepository : IGenericRepository<BoPhase>
    {
        Task<IEnumerable<BoPhase>> GetByTraineeIdAsync(int traineeId);
        Task<IEnumerable<BoPhase>> GetByIdsAsync(IEnumerable<int> ids);
        Task<IEnumerable<BoPhase>> GetBoPhaseDetailsByTraineeIds(IEnumerable<int> traineeIds);
    }
}
