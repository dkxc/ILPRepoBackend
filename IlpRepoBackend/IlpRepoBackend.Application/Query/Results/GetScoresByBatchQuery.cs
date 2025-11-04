using IlpRepoBackend.Application.Dto.Results;
using IlpRepoBackend.Application.Wrapper;
using MediatR;
using System.Collections.Generic;

namespace IlpRepoBackend.Application.Queries.Results
{
    public class GetScoresByBatchQuery : IRequest<ApiResponse<List<TraineeScoreDto>>>
    {
        public int BatchId { get; set; }
    }
}