using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrapper;
using MediatR;

namespace IlpRepoBackend.Application.Query.PhaseTypes
{
    public class GetPhaseTypeByIdQuery : IRequest<ApiResponse<PhaseTypeDto>>
    {
        public int Id { get; set; }
    }
}