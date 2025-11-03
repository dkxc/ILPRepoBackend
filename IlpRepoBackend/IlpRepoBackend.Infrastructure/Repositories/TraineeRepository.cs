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
 
        public async Task<bool> AadhaarIdExistsAsync(string aadhaarId)

        {

            if (string.IsNullOrWhiteSpace(aadhaarId))

                return false;

            return await _context.Trainees

                .AnyAsync(t => t.AadhaarId == aadhaarId);

        }
 
        public override async Task<Trainee?> GetByIdAsync(int id)

        {

            return await _context.Trainees

                .Include(t => t.User)

                .Include(t => t.Batch)

                .FirstOrDefaultAsync(t => t.Id == id);

        }
        public async Task<Trainee?> GetByName(string name)
        {
            return await _context.Trainees
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.User.Username == name);
        }

        // ✅ Required by interface, return same as method below
        public async Task<object> GetTraineesByBatchId(object batchId)
        {
            int id = Convert.ToInt32(batchId);
            var list = await GetTraineesByBatchId(id);
            return list;
        }

        public async Task<List<Trainee>> GetTraineesByBatchId(int batchId)
        {
            return await _context.Trainees
                .Where(t => t.BatchId == batchId)
                .Include(t => t.User)    // ✅ Required to get Username
                .Include(t => t.Batch)   // ✅ Required to get BatchName
                .ToListAsync();
        }
    }

}

 
