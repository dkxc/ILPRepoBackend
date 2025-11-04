using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using IlpRepoBackend.Infrastructure.Context;

namespace IlpRepoBackend.Infrastructure.Repositories
{
    public class PhaseRepository : GenericRepository<Phase>, IPhaseRepository
    {
        public PhaseRepository(AppDbContext context) : base(context)
        {
        }
    }
}