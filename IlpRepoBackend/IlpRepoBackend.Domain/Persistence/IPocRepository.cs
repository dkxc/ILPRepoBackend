using IlpRepoBackend.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Domain.Persistence
{
    public interface IPocRepository : IGenericRepository<Poc>
    {
        Task<Poc?> GetByEmailAsync(string email);
        Task<Poc?> GetByNameAsync(string v);
        Task<List<Poc>> GetPocsByProjectIdAsync(int projectId);
    }
}
