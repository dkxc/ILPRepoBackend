using IlpRepoBackend.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Domain.Persistence
{
    public interface IResultRepository : IGenericRepository<Result>
    {
        Task<IEnumerable<Result>> GetByTraineeIdAsync(int traineeId);
        Task<IEnumerable<Result>> GetByAssessmentIdAsync(int assessmentId);
    }
}
