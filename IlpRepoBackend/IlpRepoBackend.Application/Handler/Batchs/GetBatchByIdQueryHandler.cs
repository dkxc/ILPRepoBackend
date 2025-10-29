using AutoMapper;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Query.Batchs;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handler.Batchs
{
    public class GetBatchByIdQueryHandler : IRequestHandler<GetBatchByIdQuery, ApiResponse<BatchDto>>
    {
        private readonly IMapper _mapper;
        private readonly IBatchRepository _batchRepository;

        public GetBatchByIdQueryHandler(IMapper mapper, IBatchRepository batchRepository)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _batchRepository = batchRepository ?? throw new ArgumentNullException(nameof(batchRepository));
        }

        public async Task<ApiResponse<BatchDto>> Handle(GetBatchByIdQuery request, CancellationToken cancellationToken)
        {
            if (request == null || request.Id <= 0)
                return ApiResponse<BatchDto>.Fail("Invalid batch ID");

            var batch = await _batchRepository.GetByIdAsync(request.Id);
            if (batch == null)
                return ApiResponse<BatchDto>.Fail($"Batch with Id {request.Id} not found");

            var batchDto = _mapper.Map<BatchDto>(batch);
            return ApiResponse<BatchDto>.Success(batchDto);
        }
    }
}
