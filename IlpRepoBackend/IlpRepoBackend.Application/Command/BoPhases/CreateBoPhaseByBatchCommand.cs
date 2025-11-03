using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrapper;
using MediatR;
using System.Collections.Generic;

namespace IlpRepoBackend.Application.Command.BoPhases
{
    public class CreateBoPhaseByBatchCommand : IRequest<ApiResponse<List<BoPhaseDetailsDto>>>
    {
        public int BatchId { get; set; }
        public List<AddBoPhaseForBatchDto>? BoPhases { get; set; }
    }
}