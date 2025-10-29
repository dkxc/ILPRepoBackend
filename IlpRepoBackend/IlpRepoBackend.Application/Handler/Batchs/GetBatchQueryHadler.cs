using AutoMapper;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Query.Batchs;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handler.Batchs
{
    public class GetBatchQueryHadler : IRequestHandler<GetBatchsQuery, ApiResponse<List<BatchDto>>>
    {
        private readonly IMapper _iMapper;
        private readonly IBatchRepository _batchRepository;

        public GetBatchQueryHadler(IMapper iMapper, IBatchRepository batchRepository)
        {
            _iMapper = iMapper;
            _batchRepository = batchRepository;
        }

        public async Task<ApiResponse<List<BatchDto>>> Handle(GetBatchsQuery request, CancellationToken cancellationToken)
        {
            var batches = await _batchRepository.GetAllAsync(); 
            var batchDtos = _iMapper.Map<List<BatchDto>>(batches);

            return ApiResponse<List<BatchDto>>.Success(batchDtos);
        }

    }
}
