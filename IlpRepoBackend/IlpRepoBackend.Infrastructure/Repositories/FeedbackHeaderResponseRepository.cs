using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using IlpRepoBackend.Infrastructure.Context;

namespace IlpRepoBackend.Infrastructure.Repositories
{
    public class FeedbackHeaderResponseRepository : GenericRepository<FeedbackHeaderResponse>, IFeedbackHeaderResponseRepository
    {
        public FeedbackHeaderResponseRepository(AppDbContext context) : base(context)
        {
        }
    }
}