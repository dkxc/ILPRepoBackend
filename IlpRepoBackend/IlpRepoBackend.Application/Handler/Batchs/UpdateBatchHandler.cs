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
    public class UpdateBatchHandler : IRequestHandler<UpdateBatchCommand, BatchDto>
    {
        private readonly IMapper _mapper;
        private readonly IBatchRepository _batchRepository;

        public UpdateBatchHandler (IMapper mapper, IBatchRepository batchRepository)
        {
            _mapper = mapper;
            _batchRepository = batchRepository;
        }
        public Task<BatchDto> Handle(UpdateBatchCommand request, CancellationToken cancellationToken)
        {
            if (request == null || request.Id <= 0)
            {
                throw new ArgumentNullException(nameof(request), "Request or Request Id cannot be null");
            }
            var existingBatch = _batchRepository.GetByIdAsync(request.Id).Result;
            if (existingBatch == null)
            {
                throw new KeyNotFoundException($"Batch with Id {request.Id} not found");
            }
            existingBatch.BatchName = request.BatchName;
            existingBatch.BatchType = request.BatchType;
            existingBatch.Status = request.Status;
            existingBatch.StartDate = request.StartDate;
            existingBatch.EndDate = request.EndDate;
            existingBatch.UpdatedAt = DateTime.UtcNow;
            var updatedBatch = _batchRepository.UpdateAsync(existingBatch).Result;
            return Task.FromResult(_mapper.Map<BatchDto>(updatedBatch));
        }
    }
}
