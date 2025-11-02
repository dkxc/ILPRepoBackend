using IlpRepoBackend.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Domain.Persistence
{
    public interface ITraineeRepository : IGenericRepository<Trainee>
    {
        Task<List<Trainee>> GetByBatchIdAsync(int batchId);
        Task<Trainee?> GetByName(string name);
        Task<object> GetTraineesByBatchId(object batchId);
        Task<List<Trainee>> GetTraineesByBatchId(int batchId);
    }
}
