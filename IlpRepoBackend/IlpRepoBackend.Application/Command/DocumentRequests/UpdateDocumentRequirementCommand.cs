using IlpRepoBackend.Application.Dto.DocumentRequests;
using IlpRepoBackend.Application.Wrapper;
using MediatR;

namespace IlpRepoBackend.Application.Command.DocumentRequests
{
    public class UpdateDocumentRequirementCommand : IRequest<ApiResponse<DocumentRequirementResponseDto>>
    {
        public int DocumentTypeId { get; set; }
        public int BatchId { get; set; }
        public DateTime DueDate { get; set; }

        public UpdateDocumentRequirementCommand(int documentTypeId, int batchId, DateTime dueDate)
        {
            DocumentTypeId = documentTypeId;
            BatchId = batchId;
            DueDate = dueDate;
        }
    }
}