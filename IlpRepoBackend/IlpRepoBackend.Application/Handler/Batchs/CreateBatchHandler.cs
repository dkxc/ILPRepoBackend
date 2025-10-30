using AutoMapper;
using IlpRepoBackend.Application.Command.Batchs;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handler.Batchs
{
    public class CreateBatchHandler : IRequestHandler<CreateBatchCommand, BatchDto>
    {
        private readonly IMapper _mapper;
        private readonly IBatchRepository _batchRepository;
        public CreateBatchHandler(IMapper mapper, IBatchRepository batchRepository)
        {
            _mapper = mapper;
            _batchRepository = batchRepository;
        }
        public Task<BatchDto> Handle(CreateBatchCommand request, CancellationToken cancellationToken)
        {
            var batch = new Domain.Entities.Batch
            {
                BatchName = request.BatchName,
                BatchType = request.BatchType,
                Status = request.Status,
                StartDate = request.StartDate,
                EndDate = request.EndDate
            };
            var createdBatch =  _batchRepository.AddAsync(batch);
            return createdBatch.ContinueWith(t => _mapper.Map<BatchDto>(t.Result), cancellationToken);
        }
    }
}
