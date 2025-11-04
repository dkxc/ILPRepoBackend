using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using IlpRepoBackend.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IlpRepoBackend.Infrastructure.Repositories
{
    public class FeedbackRepository : GenericRepository<Feedback>, IFeedbackRepository
    {
        private readonly AppDbContext _context;
        public FeedbackRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        // This implementation now correctly matches the updated interface.
        public async Task<IEnumerable<Feedback>> GetByTraineeIdAsync(int traineeId)
        {
            return await _context.Feedbacks
                .Include(f => f.FeedbackHeaderResponses)
                    .ThenInclude(fhr => fhr.FeedbackHeader)
                .Where(f => f.TraineeId == traineeId)
                .ToListAsync();
        }
    }
}