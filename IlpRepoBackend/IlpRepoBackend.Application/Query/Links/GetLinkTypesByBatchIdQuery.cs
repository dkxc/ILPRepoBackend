using IlpRepoBackend.Application.Dto.Links;
using IlpRepoBackend.Application.Wrapper;
using MediatR;
using System.Collections.Generic;

namespace IlpRepoBackend.Application.Query.Links
{
    public class GetLinkTypesByBatchIdQuery : IRequest<ApiResponse<List<BatchLinkTypeDto>>>
    {
        public int BatchId { get; set; }

        public GetLinkTypesByBatchIdQuery(int batchId)
        {
            BatchId = batchId;
        }
    }
}