using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrapper;
using MediatR;
using System.Collections.Generic;

namespace IlpRepoBackend.Application.Query.BoPhases
{
    public class GetBoPhaseDetailsByBatchQuery : IRequest<ApiResponse<List<BoPhaseDetailsDto>>>
    {
        public int BatchId { get; set; }

        public GetBoPhaseDetailsByBatchQuery(int batchId)
        {
            BatchId = batchId;
        }
    }
}