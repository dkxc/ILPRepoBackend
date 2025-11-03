using AutoMapper;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Query.PhaseTypes;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handler.PhaseTypes
{
    public class GetPhaseTypesHandler : IRequestHandler<GetPhaseTypesQuery, ApiResponse<List<PhaseTypeDto>>>
    {
        private readonly IBatchRepository _batchRepository;
        private readonly IMapper _mapper;

        public GetPhaseTypesHandler(IBatchRepository batchRepository, IMapper mapper)
        {
            _batchRepository = batchRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<List<PhaseTypeDto>>> Handle(GetPhaseTypesQuery request, CancellationToken cancellationToken)
        {
            var phaseTypes = await _batchRepository.GetPhaseTypesAsync();
            var dtos = _mapper.Map<List<PhaseTypeDto>>(phaseTypes);
            return ApiResponse<List<PhaseTypeDto>>.Success(dtos);
        }
    }
}