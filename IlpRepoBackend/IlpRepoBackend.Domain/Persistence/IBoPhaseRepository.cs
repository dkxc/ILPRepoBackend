using IlpRepoBackend.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Domain.Persistence
{
    public interface IBoPhaseRepository : IGenericRepository<BoPhase>
    {
        Task<IEnumerable<BoPhase>> GetByTraineeIdAsync(int traineeId);
    }
}
