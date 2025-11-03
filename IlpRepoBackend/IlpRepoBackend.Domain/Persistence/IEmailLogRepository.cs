using IlpRepoBackend.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Domain.Persistence
{
    public interface IEmailLogRepository : IGenericRepository<EmailLog>
    {
        Task<IEnumerable<EmailLog>> GetByConfigurationIdAsync(int configurationId);
        Task<IEnumerable<EmailLog>> GetByRelatedEntityAsync(string entityType, int entityId);
        Task<IEnumerable<EmailLog>> GetRecentLogsAsync(int count);
        Task<bool> WasReminderSentTodayAsync(string entityType, int entityId);
    }
}
