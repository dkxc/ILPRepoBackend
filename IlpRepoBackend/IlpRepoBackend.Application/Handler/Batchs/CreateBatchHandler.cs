using AutoMapper;
using IlpRepoBackend.Application.Command.Batchs;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using IlpRepoBackend.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        
        public async Task<BatchDto> Handle(CreateBatchCommand request, CancellationToken cancellationToken)
        {
            var status = CalculateStatus(request.StartDate, request.EndDate);

            var batch = new Batch
            {
                BatchName = request.BatchName,
                BatchTypeId = request.BatchTypeId,
                Status = status,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Add phases if provided
            if (request.Phases != null && request.Phases.Any())
            {
                batch.Phases = request.Phases.Select(p => new Phase
                {
                    PhaseType = p.PhaseType,
                    PhaseTypeId = p.PhaseTypeId,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }).ToList();
            }

            // Add the batch with phases
            var created = await _batchRepository.AddAsync(batch);

            // Re-fetch with related data (BatchType and Phases) to ensure DTO has all data populated
            var createdWithIncludes = await _batchRepository.GetByIdAsync(created.Id);

            return _mapper.Map<BatchDto>(createdWithIncludes);
        }

        private BatchStatus CalculateStatus(DateTime? startDate, DateTime? endDate)
        {
            var now = DateTime.UtcNow;

            if (!startDate.HasValue && !endDate.HasValue)
            {
                return BatchStatus.NotStarted;
            }

            if (startDate.HasValue && endDate.HasValue)
            {
                if (now < startDate.Value)
                    return BatchStatus.NotStarted;
                if (now > endDate.Value)
                    return BatchStatus.Completed;
                return BatchStatus.Ongoing;
            }

            if (startDate.HasValue && !endDate.HasValue)
            {
                return now < startDate.Value ? BatchStatus.NotStarted : BatchStatus.Ongoing;
            }

            // only endDate has value
            return endDate.Value >= now ? BatchStatus.Ongoing : BatchStatus.Completed;
        }
    }
}
