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
        Task<IEnumerable<Poc>> GetByProjectIdAsync(int projectId);
    }
}
