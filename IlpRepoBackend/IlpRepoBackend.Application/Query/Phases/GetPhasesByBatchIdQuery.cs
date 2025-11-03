using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrapper;
using MediatR;
using System.Collections.Generic;

namespace IlpRepoBackend.Application.Query.Phases
{
    public class GetPhasesByBatchIdQuery : IRequest<ApiResponse<List<PhaseDto>>>
    {
        public int BatchId { get; set; }

        public GetPhasesByBatchIdQuery(int batchId)
        {
            BatchId = batchId;
        }
    }
}
