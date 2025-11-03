using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using IlpRepoBackend.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Infrastructure.Repositories
{
    public class EmailLogRepository : GenericRepository<EmailLog>, IEmailLogRepository
    {
        private readonly AppDbContext _context;
        public EmailLogRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EmailLog>> GetByConfigurationIdAsync(int configurationId)
        {
            return await _context.EmailLogs
                .Include(e => e.EmailConfiguration)
                .Where(e => e.EmailConfigurationId == configurationId)
                .OrderByDescending(e => e.SentAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<EmailLog>> GetByRelatedEntityAsync(string entityType, int entityId)
        {
            return await _context.EmailLogs
                .Where(e => e.RelatedEntityType == entityType && e.RelatedEntityId == entityId)
                .OrderByDescending(e => e.SentAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<EmailLog>> GetRecentLogsAsync(int count)
        {
            return await _context.EmailLogs
                .Include(e => e.EmailConfiguration)
                .OrderByDescending(e => e.SentAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<bool> WasReminderSentTodayAsync(string entityType, int entityId)
        {
            return await _context.EmailLogs
                .AnyAsync(log =>
                    log.RelatedEntityType == entityType &&
                    log.RelatedEntityId == entityId &&
                    log.SentAt.Date == DateTime.UtcNow.Date &&
                    log.IsSent);
        }
    }
}
