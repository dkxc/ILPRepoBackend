using MediatR;

namespace IlpRepoBackend.Application.Command.PhaseTypes
{
    public class DeletePhaseTypeCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }
}