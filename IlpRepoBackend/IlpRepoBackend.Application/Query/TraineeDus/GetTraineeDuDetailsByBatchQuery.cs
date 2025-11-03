using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrapper;
using MediatR;
using System.Collections.Generic;

namespace IlpRepoBackend.Application.Query.TraineeDus
{
    public class GetTraineeDuDetailsByBatchQuery : IRequest<ApiResponse<List<TraineeDuDetailsDto>>>
    {
        public int BatchId { get; set; }

        public GetTraineeDuDetailsByBatchQuery(int batchId)
        {
            BatchId = batchId;
        }
    }
}