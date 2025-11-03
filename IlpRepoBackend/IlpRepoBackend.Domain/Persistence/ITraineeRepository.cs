using IlpRepoBackend.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IlpRepoBackend.Domain.Persistence
{
    public interface ITraineeRepository : IGenericRepository<Trainee>
    {
        Task<IEnumerable<Trainee>> GetByBatchIdAsync(int batchId);
        Task<Trainee?> GetByUserIdAsync(int userId);
        Task<bool> AadhaarIdExistsAsync(string aadhaarId);

        //Task<List<Trainee>> GetByBatchIdAsync(int batchId);
        Task<Trainee?> GetByName(string name);
        Task<object> GetTraineesByBatchId(object batchId);
        Task<List<Trainee>> GetTraineesByBatchId(int batchId);

    }
}
