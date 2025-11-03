using IlpRepoBackend.Application.Command.PhaseTypes;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handler.PhaseTypes
{
    public class DeletePhaseTypeHandler : IRequestHandler<DeletePhaseTypeCommand, bool>
    {
        private readonly IBatchRepository _batchRepository;

        public DeletePhaseTypeHandler(IBatchRepository batchRepository)
        {
            _batchRepository = batchRepository;
        }

        public async Task<bool> Handle(DeletePhaseTypeCommand request, CancellationToken cancellationToken)
        {
            return await _batchRepository.DeletePhaseTypeAsync(request.Id);
        }
    }
}