using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using IlpRepoBackend.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace IlpRepoBackend.Infrastructure.Repositories
{
    public class BatchRepository : GenericRepository<Batch>, IBatchRepository
    {
        private readonly AppDbContext _context;

        public BatchRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public override async Task<IEnumerable<Batch>> GetAllAsync()
        {
            return await _context.Batches
                .Include(b => b.BatchType)
                .Include(b => b.Phases)
                    .ThenInclude(p => p.PhaseTypeEntity)
                .ToListAsync();
        }

        public async Task<Batch?> GetByIdAsync(int id)
        {
            return await _context.Batches
                .Include(b => b.BatchType)
                .Include(b => b.Phases)
                    .ThenInclude(p => p.PhaseTypeEntity)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<List<BatchType>> GetBatchTypesAsync()
        {
            return await _context.BatchTypes.OrderBy(bt => bt.Name).ToListAsync();
        }

        public async Task<BatchType> AddBatchTypeAsync(BatchType batchType)
        {
            var entry = await _context.BatchTypes.AddAsync(batchType);
            await _context.SaveChangesAsync();
            return entry.Entity;
        }

        public async Task<bool> BatchTypeNameExistsAsync(string name, int? excludeId = null)
        {
            var query = _context.BatchTypes.AsQueryable();
            if (excludeId.HasValue)
                query = query.Where(bt => bt.Id != excludeId.Value);
            return await query.AnyAsync(bt => bt.Name.ToLower() == name.ToLower());
        }

        public async Task<bool> DeleteBatchTypeAsync(int id)
        {
            var entity = await _context.BatchTypes.FindAsync(id);
            if (entity == null) return false;
            _context.BatchTypes.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<BatchType> UpdateBatchTypeAsync(BatchType batchType)
        {
            var existing = await _context.BatchTypes.FindAsync(batchType.Id);
            if (existing == null) return null!;
            existing.Name = batchType.Name;
            existing.UpdatedAt = batchType.UpdatedAt;
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<BatchType?> GetBatchTypeByNameAsync(string name)
        {
            return await _context.BatchTypes
                .FirstOrDefaultAsync(bt => bt.Name.ToLower() == name.ToLower());
        }

        // PhaseType implementations
        public async Task<List<PhaseType>> GetPhaseTypesAsync()
        {
            return await _context.PhaseTypes.OrderBy(pt => pt.Name).ToListAsync();
        }

        public async Task<PhaseType> AddPhaseTypeAsync(PhaseType phaseType)
        {
            var entry = await _context.PhaseTypes.AddAsync(phaseType);
            await _context.SaveChangesAsync();
            return entry.Entity;
        }

        public async Task<bool> PhaseTypeNameExistsAsync(string name, int? excludeId = null)
        {
            var query = _context.PhaseTypes.AsQueryable();
            if (excludeId.HasValue)
                query = query.Where(pt => pt.Id != excludeId.Value);
            return await query.AnyAsync(pt => pt.Name.ToLower() == name.ToLower());
        }

        public async Task<PhaseType> UpdatePhaseTypeAsync(PhaseType phaseType)
        {
            var existing = await _context.PhaseTypes.FindAsync(phaseType.Id);
            if (existing == null) return null!;
            existing.Name = phaseType.Name;
            existing.UpdatedAt = phaseType.UpdatedAt;
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeletePhaseTypeAsync(int id)
        {
            var entity = await _context.PhaseTypes.FindAsync(id);
            if (entity == null) return false;
            _context.PhaseTypes.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PhaseType?> GetPhaseTypeByIdAsync(int id)
        {
            return await _context.PhaseTypes.FindAsync(id);
        }
    }
}
