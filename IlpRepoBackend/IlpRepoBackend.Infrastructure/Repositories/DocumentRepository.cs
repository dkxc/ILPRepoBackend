using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using IlpRepoBackend.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace IlpRepoBackend.Infrastructure.Repositories
{
    public class DocumentRepository : GenericRepository<Documents>, IDocumentRepository
    {
        public DocumentRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Documents>> GetAllDocumentTypesAsync()
        {
            return await _context.Documents
                .OrderBy(d => d.Name)
                .ToListAsync();
        }

        public async Task<Documents?> GetDocumentWithLinkAsync(int documentId)
        {
            return await _context.Documents
                .FirstOrDefaultAsync(d => d.Id == documentId);
        }
    }
}