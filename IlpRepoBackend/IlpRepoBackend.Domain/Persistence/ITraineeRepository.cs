using IlpRepoBackend.Domain.Entities;

namespace IlpRepoBackend.Domain.Persistence
{
    public interface ITraineeRepository : IGenericRepository<Trainee>
    {
        Task<IEnumerable<Trainee>> GetByBatchIdAsync(int batchId);
        Task<Trainee?> GetByUserIdAsync(int userId);
        Task<bool> AadhaarIdExistsAsync(string aadhaarId);
        Task<IEnumerable<Trainee>> GetTraineesWithProjectsByBatchIdAsync(int batchId);
        Task<Trainee?> GetByName(string name);
        Task<List<Trainee>> GetTraineesWithResultsByBatchIdAsync(int batchId);
        // This was added later in development
        // due to flaw in Project & ProjectTeam design
        Task<Trainee?> GetByUserIdWithDetailsAsync(int userId);
    }
}
