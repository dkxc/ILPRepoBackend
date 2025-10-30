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
            return await _context.DocumentSubmissions
                .Include(ds => ds.Document)
                .Include(ds => ds.DocumentRequest)
                .Include(ds => ds.Project)
                .Include(ds => ds.Trainee)
                    .ThenInclude(t => t.User)
                .Where(ds => ds.RequestId == requestId)
                .OrderByDescending(ds => ds.SubmissionDate)
                .ToListAsync();
        }

        public async Task<DocumentSubmission?> GetSubmissionWithDetailsAsync(int submissionId)
        {
            return await _context.DocumentSubmissions
                .Include(ds => ds.Document)
                .Include(ds => ds.DocumentRequest)
                .Include(ds => ds.Project)
                .Include(ds => ds.Trainee)
                    .ThenInclude(t => t.User)
                .FirstOrDefaultAsync(ds => ds.Id == submissionId);
        }
    }
}