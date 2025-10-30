using IlpRepoBackend.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IlpRepoBackend.Domain.Persistence
{
    public interface IMentorForPRojectRepository : IGenericRepository<MenterForAProject>
    {
        Task<IEnumerable<MenterForAProject>> GetByProjectIdAsync(int projectId);
        Task DeleteByProjectIdAndMentorIdAsync(int projectId, int mentorId);
        Task DeleteAllByProjectIdAsync(int projectId);
    }
}