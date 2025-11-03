using AutoMapper;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Query.PhaseTypes;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handler.PhaseTypes
{
    public class GetPhaseTypeByIdHandler : IRequestHandler<GetPhaseTypeByIdQuery, ApiResponse<PhaseTypeDto>>
    {
        private readonly IBatchRepository _batchRepository;
        private readonly IMapper _mapper;

        public GetPhaseTypeByIdHandler(IBatchRepository batchRepository, IMapper mapper)
        {
            _batchRepository = batchRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<PhaseTypeDto>> Handle(GetPhaseTypeByIdQuery request, CancellationToken cancellationToken)
        {
            var phaseType = await _batchRepository.GetPhaseTypeByIdAsync(request.Id);
            
            if (phaseType == null)
                return ApiResponse<PhaseTypeDto>.Fail($"Phase type with ID {request.Id} not found");

            var dto = _mapper.Map<PhaseTypeDto>(phaseType);
            return ApiResponse<PhaseTypeDto>.Success(dto);
        }
    }
}