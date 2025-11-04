using IlpRepoBackend.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Domain.Persistence
{
    public interface ITrainingScheduleRepository : IGenericRepository<TrainingSchedule>
    {
        Task<IEnumerable<TrainingSchedule>> GetByBatchIdAsync(int batchId);
        Task AddRangeAsync(IEnumerable<TrainingSchedule> schedules);
        Task<IEnumerable<TrainingSchedule>> GetByBatchTypeAndDateRangeAsync(int? batchTypeId, DateTime startDate, DateTime endDate);
    }
}
