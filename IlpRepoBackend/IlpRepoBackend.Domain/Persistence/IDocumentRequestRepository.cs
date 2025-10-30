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
        Task<int?> GetProjectIdByBatchIdAsync(int batchId);
        Task<Documents?> GetDocumentByIdAsync(int documentId);
        Task<Project?> GetProjectByIdAsync(int projectId);
        Task<DocumentRequest?> GetDocumentRequestWithDetailsAsync(int id);
    }
}
