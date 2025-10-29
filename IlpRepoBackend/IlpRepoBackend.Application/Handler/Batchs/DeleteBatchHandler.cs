using IlpRepoBackend.Application.Command.Batchs;
using IlpRepoBackend.Application.Command.Users;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handler.Batchs
{
    internal class DeleteBatchHandler : IRequestHandler<DeleteBatchCommand, bool>
    {
        private readonly IMediator _mediator;
        private readonly IBatchRepository _batchRepository;
        public DeleteBatchHandler(IMediator mediator, IBatchRepository batchRepository)
        {
            _mediator = mediator;
            _batchRepository = batchRepository;
        }
       

        public async Task<bool> Handle(DeleteBatchCommand request, CancellationToken cancellationToken)
        {
            var batch =  _batchRepository.GetByIdAsync(request.Id);
            if (batch == null)
            {
                throw new InvalidOperationException($"Batch with ID '{request.Id}' not found");
            }
            return  await _batchRepository.DeleteAsync(request.Id);

        }
    }
}
