using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrapper;
using MediatR;
using System.Collections.Generic;

namespace IlpRepoBackend.Application.Query
{
    public class GetCurriculumByBatchIdQuery : IRequest<ApiResponse<List<CurriculumDto>>>
    {
        public int BatchId { get; set; }
    }
}
