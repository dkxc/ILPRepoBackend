using IlpRepoBackend.Application.Command.DocumentRequests;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IlpRepoBackend.Application.Handler.DocumentRequests
{
    public class DeleteDocumentRequirementHandler : IRequestHandler<DeleteDocumentRequirementCommand, ApiResponse<bool>>
    {
        private readonly IDocumentRequestRepository _documentRequestRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IDocumentRepository _documentRepository;
        private readonly ILogger<DeleteDocumentRequirementHandler> _logger;

        public DeleteDocumentRequirementHandler(
            IDocumentRequestRepository documentRequestRepository,
            IProjectRepository projectRepository,
            IDocumentRepository documentRepository,
            ILogger<DeleteDocumentRequirementHandler> logger)
        {
            _documentRequestRepository = documentRequestRepository;
            _projectRepository = projectRepository;
            _documentRepository = documentRepository;
            _logger = logger;
        }

        public async Task<ApiResponse<bool>> Handle(DeleteDocumentRequirementCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Validate document type exists
                var documentType = await _documentRepository.GetByIdAsync(request.DocumentTypeId);
                if (documentType == null)
                {
                    return ApiResponse<bool>.Fail($"Document type with ID {request.DocumentTypeId} not found");
                }

                // Get all projects for the given batch
                var projects = await _projectRepository.GetProjectsByBatchIdAsync(request.BatchId);
                var projectsList = projects.ToList();

                if (!projectsList.Any())
                {
                    return ApiResponse<bool>.Fail($"No projects found for batch ID {request.BatchId}");
                }

                var projectIds = projectsList.Select(p => p.Id).ToList();

                // Get existing document requests for this batch and document type
                var existingRequests = await _documentRequestRepository.GetByDocumentIdAndProjectIdsAsync(request.DocumentTypeId, projectIds);
                var existingRequestsList = existingRequests.ToList();

                if (!existingRequestsList.Any())
                {
                    return ApiResponse<bool>.Fail($"No document requirement found for '{documentType.Name}' in batch {request.BatchId}");
                }

                // Delete all document requests for this batch and document type
                int deletedCount = 0;
                foreach (var docRequest in existingRequestsList)
                {
                    var success = await _documentRequestRepository.DeleteAsync(docRequest.Id);
                    if (success) deletedCount++;
                }

                _logger.LogInformation($"Deleted document requirement '{documentType.Name}' for batch {request.BatchId}, removed {deletedCount} document requests");

                return ApiResponse<bool>.Success(true, 
                    $"Document requirement deleted successfully. Removed {deletedCount} document requests from batch {request.BatchId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document requirement for batch {BatchId}", request.BatchId);
                return ApiResponse<bool>.Fail($"Failed to delete document requirement: {ex.Message}");
            }
        }
    }
}