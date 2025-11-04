using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using IlpRepoBackend.Infrastructure.Context;

namespace IlpRepoBackend.Infrastructure.Repositories
{
    public class FeedbackHeaderRepository : GenericRepository<FeedbackHeader>, IFeedbackHeaderRepository
    {
        public FeedbackHeaderRepository(AppDbContext context) : base(context)
        {
        }
    }
}