using IlpRepoBackend.Application.Wrapper;
using MediatR;

namespace IlpRepoBackend.Application.Command.DocumentRequests
{
    public class DeleteDocumentRequirementCommand : IRequest<ApiResponse<bool>>
    {
        public int DocumentTypeId { get; set; }
        public int BatchId { get; set; }

        public DeleteDocumentRequirementCommand(int documentTypeId, int batchId)
        {
            DocumentTypeId = documentTypeId;
            BatchId = batchId;
        }
    }
}