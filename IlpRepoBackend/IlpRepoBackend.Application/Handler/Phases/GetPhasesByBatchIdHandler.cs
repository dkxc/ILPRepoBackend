using AutoMapper;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Query.Phases;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handler.Phases
{
    public class GetPhasesByBatchIdHandler : IRequestHandler<GetPhasesByBatchIdQuery, ApiResponse<List<PhaseDto>>>
    {
        private readonly IBatchRepository _batchRepository;
        private readonly IMapper _mapper;

        public GetPhasesByBatchIdHandler(IBatchRepository batchRepository, IMapper mapper)
        {
            _batchRepository = batchRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<List<PhaseDto>>> Handle(GetPhasesByBatchIdQuery request, CancellationToken cancellationToken)
        {
            // Validate batch exists
            var batch = await _batchRepository.GetByIdAsync(request.BatchId);
            if (batch == null)
                return ApiResponse<List<PhaseDto>>.Fail($"Batch with ID {request.BatchId} not found");

            // Check if batch has phases
            if (batch.Phases == null || !batch.Phases.Any())
                return ApiResponse<List<PhaseDto>>.Fail("No phases found for the specified batch");

            var phaseDtos = _mapper.Map<List<PhaseDto>>(batch.Phases);
            return ApiResponse<List<PhaseDto>>.Success(phaseDtos);
        }
    }
}
