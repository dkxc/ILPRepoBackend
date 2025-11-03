using IlpRepoBackend.Domain.Entities;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace IlpRepoBackend.Domain.Persistence
{
    public interface IBatchRepository : IGenericRepository<Batch>
    {
        Task<Batch?> GetByIdAsync(int id);
        Task<List<BatchType>> GetBatchTypesAsync();
        Task<BatchType> AddBatchTypeAsync(BatchType batchType);
        Task<bool> BatchTypeNameExistsAsync(string name, int? excludeId = null);
        Task<bool> DeleteBatchTypeAsync(int id);
        Task<BatchType> UpdateBatchTypeAsync(BatchType batchType);
        Task<BatchType?> GetBatchTypeByNameAsync(string name);
        Task<List<PhaseType>> GetPhaseTypesAsync();
        Task<PhaseType> AddPhaseTypeAsync(PhaseType phaseType);
        Task<bool> PhaseTypeNameExistsAsync(string name, int? excludeId = null);
        Task<PhaseType> UpdatePhaseTypeAsync(PhaseType phaseType);
        Task<bool> DeletePhaseTypeAsync(int id);
        Task<PhaseType?> GetPhaseTypeByIdAsync(int id);
    }
}
