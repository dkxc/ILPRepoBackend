using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using IlpRepoBackend.Infrastructure.Context;

namespace IlpRepoBackend.Infrastructure.Repositories
{
    public class LinkRepository : GenericRepository<Link>, ILinkRepository
    {
        public LinkRepository(AppDbContext context) : base(context)
        {
        }
    }
}