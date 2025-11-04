using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrapper;
using MediatR;
using System.Collections.Generic;

namespace IlpRepoBackend.Application.Query.Batches
{
    public class GetSpecializationPhaseByBatchQuery : IRequest<ApiResponse<List<SpecializationPhaseDto>>>
    {
        public int BatchId { get; set; }

        public GetSpecializationPhaseByBatchQuery(int batchId)
        {
            BatchId = batchId;
        }
    }
}