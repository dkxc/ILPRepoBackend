using AutoMapper;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Query.BatchTypes;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handler.BatchTypes
{
    public class GetBatchTypesHandler : IRequestHandler<GetBatchTypesQuery, ApiResponse<List<BatchTypeDto>>>
    {
        private readonly IBatchRepository _batchRepository;
        private readonly IMapper _mapper;

        public GetBatchTypesHandler(IBatchRepository batchRepository, IMapper mapper)
        {
            _batchRepository = batchRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<List<BatchTypeDto>>> Handle(GetBatchTypesQuery request, CancellationToken cancellationToken)
        {
            var batchTypes = await _batchRepository.GetBatchTypesAsync();
            var dtos = _mapper.Map<List<BatchTypeDto>>(batchTypes);
            return ApiResponse<List<BatchTypeDto>>.Success(dtos);
        }
    }
}