using AutoMapper;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Query;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handler
{
    public class GetCurriculumByBatchIdQueryHandler : IRequestHandler<GetCurriculumByBatchIdQuery, ApiResponse<List<CurriculumDto>>>
    {
        private readonly ICurriculumRepository _curriculumRepository;
        private readonly IMapper _mapper;

        public GetCurriculumByBatchIdQueryHandler(ICurriculumRepository curriculumRepository, IMapper mapper)
        {
            _curriculumRepository = curriculumRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<List<CurriculumDto>>> Handle(GetCurriculumByBatchIdQuery request, CancellationToken cancellationToken)
        {
            var curriculum = await _curriculumRepository.GetByBatchIdAsync(request.BatchId);
            var curriculumDto = _mapper.Map<List<CurriculumDto>>(curriculum);
            return ApiResponse<List<CurriculumDto>>.Success(curriculumDto);
        }
    }
}
