using AutoMapper;
using IlpRepoBackend.Application.Command;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handler
{
    public class UpdateCurriculumCommandHandler : IRequestHandler<UpdateCurriculumCommand, ApiResponse<CurriculumDto>>
    {
        private readonly ICurriculumRepository _curriculumRepository;
        private readonly IBatchRepository _batchRepository;
        private readonly IMapper _mapper;

        public UpdateCurriculumCommandHandler(ICurriculumRepository curriculumRepository, IBatchRepository batchRepository, IMapper mapper)
        {
            _curriculumRepository = curriculumRepository;
            _batchRepository = batchRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<CurriculumDto>> Handle(UpdateCurriculumCommand request, CancellationToken cancellationToken)
        {
            // 1. Verify the batch exists
            var batchExists = await _batchRepository.GetByIdAsync(request.BatchId) != null;
            if (!batchExists)
            {
                return ApiResponse<CurriculumDto>.Fail("Batch not found.");
            }

            // 2. Find the existing curriculum event
            var existingCurriculum = await _curriculumRepository.GetByIdAsync(request.Id);

            // 3. Validate that it exists and belongs to the correct batch
            if (existingCurriculum == null || existingCurriculum.BatchId != request.BatchId)
            {
                return ApiResponse<CurriculumDto>.Fail("Curriculum event not found in this batch.");
            }

            // 4. Map the changes from the DTO to the entity and update
            _mapper.Map(request.UpdateCurriculumDto, existingCurriculum);
            await _curriculumRepository.UpdateAsync(existingCurriculum);

            // 5. Map the updated entity back to a DTO for the response
            var updatedCurriculumDto = _mapper.Map<CurriculumDto>(existingCurriculum);

            return ApiResponse<CurriculumDto>.Success(updatedCurriculumDto);
        }
    }
}
