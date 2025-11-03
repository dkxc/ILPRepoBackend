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
    public class EmailConfigurationRepository : GenericRepository<EmailConfiguration>, IEmailConfigurationRepository
    {
        private readonly AppDbContext _context;
        public EmailConfigurationRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<EmailConfiguration?> GetByServiceNameAsync(string serviceName)
        {
            return await _context.EmailConfigurations
                .FirstOrDefaultAsync(e => e.ServiceName == serviceName);
        }

        public async Task<IEnumerable<EmailConfiguration>> GetActiveConfigurationsAsync()
        {
            return await _context.EmailConfigurations
                .Where(e => e.IsActive)
                .ToListAsync();
        }
    }
}
