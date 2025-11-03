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
    public class DocumentRequestRepository : GenericRepository<DocumentRequest>, IDocumentRequestRepository
    {
        public DocumentRequestRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<DocumentRequest>> GetByProjectIdAsync(int projectId)
        {
            return await _dbSet
                .Where(dr => dr.ProjectId == projectId)
                .Include(dr => dr.Document)
                .Include(dr => dr.Project)
                .ToListAsync();
        }

        public async Task<IEnumerable<DocumentRequest>> GetByDocumentIdAndProjectIdsAsync(int documentId, List<int> projectIds)
        {
            return await _dbSet
                .Where(dr => dr.DocumentId == documentId && projectIds.Contains(dr.ProjectId.Value))
                .Include(dr => dr.Document)
                .Include(dr => dr.Project)
                .ToListAsync();
        }

        public async Task<DocumentRequest?> GetByIdWithIncludesAsync(int id)
        {
            return await _dbSet
                .Where(dr => dr.Id == id)
                .Include(dr => dr.Document)
                .Include(dr => dr.Project)
                .FirstOrDefaultAsync();
        }
    }
}
        private readonly AppDbContext _context;
        public DocumentRequestRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public Task<IEnumerable<DocumentRequest>> GetByProjectIdAsync(int projectId)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<DocumentRequest>> GetPendingRequestsDueByDateAsync(DateTime dueDate)
        {
            return await _context.DocumentRequests
                .Include(dr => dr.Document)
                .Include(dr => dr.Project)
                    .ThenInclude(p => p.ProjectTeams)
                        .ThenInclude(pt => pt.Trainee)
                            .ThenInclude(t => t.User)
                .Include(dr => dr.DocumentSubmissions)
                .Where(dr => dr.DueDate.Date == dueDate.Date &&
                             (dr.DocumentSubmissions == null || !dr.DocumentSubmissions.Any()))
                .ToListAsync();
        }

        public async Task<DocumentRequest?> GetWithDetailsAsync(int id)
        {
            return await _context.DocumentRequests
                .Include(dr => dr.Document)
                .Include(dr => dr.Project)
                    .ThenInclude(p => p.ProjectTeams)
                        .ThenInclude(pt => pt.Trainee)
                            .ThenInclude(t => t.User)
                .Include(dr => dr.DocumentSubmissions)
                .FirstOrDefaultAsync(dr => dr.Id == id);
        }
    }
}
