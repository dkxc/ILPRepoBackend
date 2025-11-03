using IlpRepoBackend.Application.Dto.DocumentRequests;
using IlpRepoBackend.Application.Wrapper;
using MediatR;

namespace IlpRepoBackend.Application.Query.DocumentRequests
{
    public class GetDocumentRequirementsByBatchIdQuery : IRequest<ApiResponse<List<DocumentRequirementResponseDto>>>
    {
        public int BatchId { get; set; }

        public GetDocumentRequirementsByBatchIdQuery(int batchId)
        {
            BatchId = batchId;
        }
    }
}