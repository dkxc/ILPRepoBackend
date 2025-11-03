using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using IlpRepoBackend.Infrastructure.Context;

namespace IlpRepoBackend.Infrastructure.Repositories
{
    public class DocumentRepository : GenericRepository<Documents>, IDocumentRepository
    {
        public DocumentRepository(AppDbContext context) : base(context)
        {
        }
    }
}