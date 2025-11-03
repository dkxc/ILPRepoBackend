using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using IlpRepoBackend.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace IlpRepoBackend.Infrastructure.Repositories
{
    public class DocumentSubmissionRepository : GenericRepository<DocumentSubmission>, IDocumentSubmissionRepository
    {
        public DocumentSubmissionRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<DocumentSubmission>> GetByRequestIdAsync(int requestId)
        {
            return await _dbSet
                .Where(ds => ds.RequestId == requestId)
                .Include(ds => ds.Document)
                .Include(ds => ds.DocumentRequest)
                .ToListAsync();
        }

        public async Task<IEnumerable<DocumentSubmission>> GetByProjectIdAsync(int projectId)
        {
            return await _dbSet
                .Where(ds => ds.DocumentRequest != null && ds.DocumentRequest.ProjectId == projectId)
                .Include(ds => ds.Document)
                .Include(ds => ds.DocumentRequest)
                    .ThenInclude(dr => dr.Project)
                .OrderByDescending(ds => ds.SubmissionDate)
                .ToListAsync();
        }
    }
}