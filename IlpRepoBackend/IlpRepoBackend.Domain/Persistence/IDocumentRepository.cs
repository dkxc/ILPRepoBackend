using IlpRepoBackend.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Domain.Persistence
{
    public interface IDocumentRepository : IGenericRepository<Documents>
    {
    }
}
