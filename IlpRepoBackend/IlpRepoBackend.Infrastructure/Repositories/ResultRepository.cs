using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using IlpRepoBackend.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IlpRepoBackend.Infrastructure.Repositories
{
    public class ResultRepository : GenericRepository<Result>, IResultRepository
    {
        private readonly AppDbContext _context;

        public ResultRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets all results for a specific assessment, including trainee details.
        /// </summary>
        public async Task<IEnumerable<Result>> GetByAssessmentIdAsync(int assessmentId)
        {
            return await _context.Results
                .Include(r => r.Trainee)
                    .ThenInclude(t => t.User)
                .Where(r => r.AssessmentId == assessmentId)
                .ToListAsync();
        }

        /// <summary>
        /// Gets all results for a specific trainee, including assessment details.
        /// </summary>
        public async Task<IEnumerable<Result>> GetByTraineeIdAsync(int traineeId)
        {
            return await _context.Results
                .Include(r => r.Assessment) // Eagerly load the Assessment details
                .Where(r => r.TraineeId == traineeId)
                .ToListAsync();
        }
    }
}