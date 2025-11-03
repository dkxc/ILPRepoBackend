using IlpRepoBackend.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Domain.Persistence
{
    public interface IDocumentRequestRepository : IGenericRepository<DocumentRequest>
    {
        Task<IEnumerable<DocumentRequest>> GetByProjectIdAsync(int projectId);
        Task<IEnumerable<DocumentRequest>> GetByDocumentIdAndProjectIdsAsync(int documentId, List<int> projectIds);
        Task<DocumentRequest?> GetByIdWithIncludesAsync(int id);
        Task<IEnumerable<DocumentRequest>> GetPendingRequestsDueByDateAsync(DateTime dueDate);
        Task<DocumentRequest?> GetWithDetailsAsync(int id);
        //Task<IEnumerable<DocumentRequest>> GetPendingRequestsDueByDateAsync(DateTime dueDate);
        //Task<DocumentRequest?> GetWithDetailsAsync(int id);
    }
}
