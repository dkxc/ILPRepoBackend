using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrapper;
using MediatR;

namespace IlpRepoBackend.Application.Command.PhaseTypes
{
    public class CreatePhaseTypeCommand : IRequest<ApiResponse<PhaseTypeDto>>
    {
        public string Name { get; set; } = string.Empty;
    }
}