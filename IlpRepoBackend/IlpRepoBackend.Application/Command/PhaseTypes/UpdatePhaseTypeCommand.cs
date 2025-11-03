using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrapper;
using MediatR;

namespace IlpRepoBackend.Application.Command.PhaseTypes
{
    public class UpdatePhaseTypeCommand : IRequest<ApiResponse<PhaseTypeDto>>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}