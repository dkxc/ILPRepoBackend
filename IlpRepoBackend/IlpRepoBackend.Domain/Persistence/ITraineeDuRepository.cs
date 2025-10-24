using IlpRepoBackend.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Domain.Persistence
{
    public interface ITraineeDuRepository : IGenericRepository<TraineeDu>
    {
        Task<IEnumerable<TraineeDu>> GetByTraineeIdAsync(int traineeId);
    }
}
