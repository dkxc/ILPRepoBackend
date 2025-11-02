using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using IlpRepoBackend.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

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