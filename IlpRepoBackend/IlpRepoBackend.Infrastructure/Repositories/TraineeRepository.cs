using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using IlpRepoBackend.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IlpRepoBackend.Infrastructure.Repositories
{
    public class TraineeRepository : GenericRepository<Trainee>, ITraineeRepository
    {
        private readonly AppDbContext _context;

        public TraineeRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public override async Task<IEnumerable<Trainee>> GetAllAsync()
        {
            return await _context.Trainees
                .Include(t => t.User)
                .Include(t => t.Batch)
                .ToListAsync();
        }
        //public async Task<List<Trainee>> GetByBatchIdAsync(int batchId)
        //{
        //    return await _context.Trainees
        //        .Where(t => t.BatchId == batchId)
        //        .Include(t => t.User)
        //        .Include(t => t.Batch)
        //        .ToListAsync();
        //}

        public async Task<IEnumerable<Trainee>> GetByBatchIdAsync(int batchId)
        {
            return await _context.Trainees
                .Include(t => t.User)
                .Include(t => t.Batch)
                .Where(t => t.BatchId == batchId)
                .ToListAsync();
        }

        public async Task<Trainee?> GetByUserIdAsync(int userId)
        {
            return await _context.Trainees
                .Include(t => t.User)
                .Include(t => t.Batch)
                .FirstOrDefaultAsync(t => t.UserId == userId);
        }

        // this was added later in dev
        // due to flaw in Project & ProjectTeams design
        public async Task<Trainee?> GetByUserIdWithDetailsAsync(int userId)
        {
            return await _context.Trainees
                .Include(t => t.User)
                .Include(t => t.Batch)
                    .ThenInclude(b => b.BatchType)
                .Include(t => t.ProjectTeams)
                    .ThenInclude(pt => pt.Project)
                .Include(t => t.Results)
                    .ThenInclude(r => r.Assessment)
                .FirstOrDefaultAsync(t => t.UserId == userId);
        }

        public async Task<bool> AadhaarIdExistsAsync(string aadhaarId)
        {
            if (string.IsNullOrWhiteSpace(aadhaarId))
                return false;

            return await _context.Trainees
                .AnyAsync(t => t.AadhaarId == aadhaarId);
        }

        public async Task<IEnumerable<Trainee>> GetTraineesWithProjectsByBatchIdAsync(int batchId)
        {
            return await _context.Trainees
                .Include(t => t.User)
                .Include(t => t.ProjectTeams)
                    .ThenInclude(pt => pt.Project)
                .Where(t => t.BatchId == batchId)
                .ToListAsync();
        }

        public async Task<Trainee?> GetByName(string name)
        {
            return await _context.Trainees
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.User.Username == name);
        }

        public override async Task<Trainee?> GetByIdAsync(int id)
        {
            return await _context.Trainees
                .Include(t => t.User)
                .Include(t => t.Batch)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<List<Trainee>> GetTraineesWithResultsByBatchIdAsync(int batchId)
        {
            return await _context.Trainees
                .Include(t => t.Results)
                .Where(t => t.BatchId == batchId)
                .ToListAsync();
        }
    }
}
