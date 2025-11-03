using IlpRepoBackend.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Domain.Persistence
{
    public interface IMentorRepository : IGenericRepository<Mentor>
    {
        // FIXED: Added Task<Mentor?> return type
        Task<Mentor?> GetByEmailAsync(string? email);

        Task<Mentor?> GetByNameAsync(string v);

        Task<IEnumerable<Mentor>> GetByProjectIdAsync(int projectId);
    }
}