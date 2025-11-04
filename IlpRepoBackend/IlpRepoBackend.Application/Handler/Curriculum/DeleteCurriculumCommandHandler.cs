using IlpRepoBackend.Application.Command;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handler
{
    public class DeleteCurriculumCommandHandler : IRequestHandler<DeleteCurriculumCommand, ApiResponse<bool>>
    {
        private readonly ICurriculumRepository _curriculumRepository;

        public DeleteCurriculumCommandHandler(ICurriculumRepository curriculumRepository)
        {
            _curriculumRepository = curriculumRepository;
        }

        public async Task<ApiResponse<bool>> Handle(DeleteCurriculumCommand request, CancellationToken cancellationToken)
        {
            // 1. Find the curriculum event to delete
            var curriculumToDelete = await _curriculumRepository.GetByIdAsync(request.Id);

            // 2. Validate that it exists and belongs to the specified batch
            if (curriculumToDelete == null || curriculumToDelete.BatchId != request.BatchId)
            {
                return ApiResponse<bool>.Fail("Curriculum event not found in this batch.");
            }

            // 3. Delete the event and save changes
            await _curriculumRepository.DeleteAsync(curriculumToDelete.Id);

            // 4. Return success response
            return ApiResponse<bool>.Success(true, "Curriculum event deleted successfully.");
        }
    }
}
