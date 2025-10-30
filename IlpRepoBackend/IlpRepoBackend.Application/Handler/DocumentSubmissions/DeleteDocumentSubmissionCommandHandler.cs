using IlpRepoBackend.Application.Command;
using IlpRepoBackend.Application.CustomException;
using IlpRepoBackend.Application.Services;
using IlpRepoBackend.Application.Wrappers;
using IlpRepoBackend.Domain.Persistence;
using MediatR;

namespace IlpRepoBackend.Application.Handler.DocumentSubmissions
{
    public class DeleteDocumentSubmissionCommandHandler : IRequestHandler<DeleteDocumentSubmissionCommand, ApiResponse<bool>>
    {
        private readonly IDocumentSubmissionRepository _documentSubmissionRepository;
        private readonly IProjectAuthorizationService _authorizationService;

        public DeleteDocumentSubmissionCommandHandler(
            IDocumentSubmissionRepository documentSubmissionRepository,
            IProjectAuthorizationService authorizationService)
        {
            _documentSubmissionRepository = documentSubmissionRepository;
            _authorizationService = authorizationService;
        }

        public async Task<ApiResponse<bool>> Handle(DeleteDocumentSubmissionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if submission exists
                var submission = await _documentSubmissionRepository.GetSubmissionWithDetailsAsync(request.SubmissionId);

                if (submission == null)
                {
                    throw new NotFoundException("Document Submission", request.SubmissionId);
                }

                // AUTHORIZATION: Check if the trainee who created this submission is Team Leader
                if (!submission.ProjectId.HasValue || !submission.TraineeId.HasValue)
                {
                    return new ApiResponse<bool>("Cannot delete submission: Missing project or trainee information", 400);
                }

                var isTeamLeader = await _authorizationService.IsTeamLeaderAsync(submission.TraineeId.Value, submission.ProjectId.Value);
                if (!isTeamLeader)
                {
                    return new ApiResponse<bool>(
                        "Unauthorized: Only the Team Leader who submitted this document can delete it",
                        403);
                }

                var fileName = submission.FileName ?? "file";

                // Delete the database record
                var deleted = await _documentSubmissionRepository.DeleteAsync(request.SubmissionId);

                if (!deleted)
                {
                    return new ApiResponse<bool>("Failed to delete document submission", 500);
                }

                return new ApiResponse<bool>(true, $"Document submission '{fileName}' deleted successfully");
            }
            catch (NotFoundException ex)
            {
                return new ApiResponse<bool>(ex.Message, 404);
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>($"An error occurred while deleting document submission: {ex.Message}", 500);
            }
        }
    }
}