using AutoMapper;
using IlpRepoBackend.Application.Command;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handler
{
    public class CreateCurriculumCommandHandler : IRequestHandler<CreateCurriculumCommand, ApiResponse<CurriculumDto>>
    {
        private readonly ICurriculumRepository _curriculumRepository;
        private readonly IBatchRepository _batchRepository;
        private readonly IMapper _mapper;

        public CreateCurriculumCommandHandler(ICurriculumRepository curriculumRepository, IBatchRepository batchRepository, IMapper mapper)
        {
            _curriculumRepository = curriculumRepository;
            _batchRepository = batchRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<CurriculumDto>> Handle(CreateCurriculumCommand request, CancellationToken cancellationToken)
        {
            var batch = await _batchRepository.GetByIdAsync(request.BatchId);
            if (batch == null)
            {
                return ApiResponse<CurriculumDto>.Fail("Batch not found.");
            }

            var curriculum = _mapper.Map<Curriculum>(request.CreateCurriculumDto);
            curriculum.BatchId = request.BatchId;

            var newCurriculum = await _curriculumRepository.AddAsync(curriculum);
            var curriculumDto = _mapper.Map<CurriculumDto>(newCurriculum);

            return ApiResponse<CurriculumDto>.Success(curriculumDto);
        }
    }
}
