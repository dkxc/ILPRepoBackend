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
            return await _context.DocumentRequests
                .Include(dr => dr.Project)
                .Include(dr => dr.Document)
                .Include(dr => dr.DocumentSubmissions)
                .Where(dr => dr.ProjectId == projectId)
                .ToListAsync();
        }

        public async Task<int?> GetProjectIdByBatchIdAsync(int batchId)
        {
            // Find a project that has trainees from the specified batch
            var projectId = await _context.ProjectTeams
                .Include(pt => pt.Trainee)
                .Where(pt => pt.Trainee.BatchId == batchId)
                .Select(pt => pt.ProjectId)
                .FirstOrDefaultAsync();

            return projectId == 0 ? null : projectId;
        }

        public async Task<Documents?> GetDocumentByIdAsync(int documentId)
        {
            return await _context.Documents
                .FirstOrDefaultAsync(d => d.Id == documentId);
        }

        public async Task<Project?> GetProjectByIdAsync(int projectId)
        {
            return await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == projectId);
        }

        public async Task<DocumentRequest?> GetDocumentRequestWithDetailsAsync(int id)
        {
            return await _context.DocumentRequests
                .Include(dr => dr.Project)
                .Include(dr => dr.Document)
                .FirstOrDefaultAsync(dr => dr.Id == id);
        }
    }
}