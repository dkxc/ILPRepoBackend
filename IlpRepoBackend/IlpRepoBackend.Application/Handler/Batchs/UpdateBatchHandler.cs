using AutoMapper;
using IlpRepoBackend.Application.Command.Batchs;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Enum;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handler.Batchs
{
    public class UpdateBatchHandler : IRequestHandler<UpdateBatchCommand, BatchDto>
    {
        private readonly IMapper _mapper;
        private readonly IBatchRepository _batchRepository;

        public UpdateBatchHandler(IMapper mapper, IBatchRepository batchRepository)
        {
            _mapper = mapper;
            _batchRepository = batchRepository;
        }

        public async Task<BatchDto> Handle(UpdateBatchCommand request, CancellationToken cancellationToken)
        {
            if (request == null || request.Id <= 0)
            {
                throw new ArgumentNullException(nameof(request), "Request or Request Id cannot be null");
            }

            var existingBatch = await _batchRepository.GetByIdAsync(request.Id);
            if (existingBatch == null)
            {
                throw new KeyNotFoundException($"Batch with Id {request.Id} not found");
            }

            // Auto-calculate status based on dates
            var calculatedStatus = CalculateStatus(request.StartDate, request.EndDate);

            // Update basic batch properties
            existingBatch.BatchName = request.BatchName;
            existingBatch.BatchTypeId = request.BatchTypeId;
            existingBatch.Status = calculatedStatus; // Use calculated status
            existingBatch.StartDate = request.StartDate;
            existingBatch.EndDate = request.EndDate;
            existingBatch.UpdatedAt = DateTime.UtcNow;

            // Handle phases update
            if (request.Phases != null)
            {
                // Clear existing phases
                existingBatch.Phases.Clear();

                // Add new phases
                if (request.Phases.Any())
                {
                    foreach (var phaseDto in request.Phases)
                    {
                        var phase = new Phase
                        {
                            PhaseType = phaseDto.PhaseType,
                            PhaseTypeId = phaseDto.PhaseTypeId,
                            StartDate = phaseDto.StartDate,
                            EndDate = phaseDto.EndDate,
                            BatchId = existingBatch.Id,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        existingBatch.Phases.Add(phase);
                    }
                }
            }

            var updatedBatch = await _batchRepository.UpdateAsync(existingBatch);
            
            // Re-fetch with includes to get complete data
            var batchWithIncludes = await _batchRepository.GetByIdAsync(updatedBatch.Id);
            
            return _mapper.Map<BatchDto>(batchWithIncludes);
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
