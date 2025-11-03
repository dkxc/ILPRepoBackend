using IlpRepoBackend.Application.Command.DocumentRequests;
using IlpRepoBackend.Application.Dto.DocumentRequests;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IlpRepoBackend.Application.Handler.DocumentRequests
{
    public class UpdateDocumentRequirementHandler : IRequestHandler<UpdateDocumentRequirementCommand, ApiResponse<DocumentRequirementResponseDto>>
    {
        private readonly IDocumentRequestRepository _documentRequestRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IDocumentRepository _documentRepository;
        private readonly ILogger<UpdateDocumentRequirementHandler> _logger;

        public UpdateDocumentRequirementHandler(
            IDocumentRequestRepository documentRequestRepository,
            IProjectRepository projectRepository,
            IDocumentRepository documentRepository,
            ILogger<UpdateDocumentRequirementHandler> logger)
        {
            _documentRequestRepository = documentRequestRepository;
            _projectRepository = projectRepository;
            _documentRepository = documentRepository;
            _logger = logger;
        }

        public async Task<ApiResponse<DocumentRequirementResponseDto>> Handle(UpdateDocumentRequirementCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Validate document type exists
                var documentType = await _documentRepository.GetByIdAsync(request.DocumentTypeId);
                if (documentType == null)
                {
                    return ApiResponse<DocumentRequirementResponseDto>.Fail($"Document type with ID {request.DocumentTypeId} not found");
                }

                // Get all projects for the given batch
                var projects = await _projectRepository.GetProjectsByBatchIdAsync(request.BatchId);
                var projectsList = projects.ToList();

                if (!projectsList.Any())
                {
                    return ApiResponse<DocumentRequirementResponseDto>.Fail($"No projects found for batch ID {request.BatchId}");
                }

                var projectIds = projectsList.Select(p => p.Id).ToList();

                // Get existing document requests for this batch and document type
                var existingRequests = await _documentRequestRepository.GetByDocumentIdAndProjectIdsAsync(request.DocumentTypeId, projectIds);
                var existingRequestsList = existingRequests.ToList();

                if (!existingRequestsList.Any())
                {
                    return ApiResponse<DocumentRequirementResponseDto>.Fail($"No document requirement found for '{documentType.Name}' in batch {request.BatchId}");
                }

                // Update due dates for all existing requests
                foreach (var docRequest in existingRequestsList)
                {
                    docRequest.DueDate = request.DueDate;
                    docRequest.UpdatedAt = DateTime.UtcNow;
                    await _documentRequestRepository.UpdateAsync(docRequest);
                }

                _logger.LogInformation($"Updated document requirement '{documentType.Name}' for batch {request.BatchId} affecting {existingRequestsList.Count} projects");

                var response = new DocumentRequirementResponseDto
                {
                    DocumentTypeId = request.DocumentTypeId,
                    DocumentTypeName = documentType.Name,
                    BatchId = request.BatchId,
                    DueDate = request.DueDate,
                    RequestDate = existingRequestsList.First().RequestDate,
                    ProjectIds = projectIds,
                    TotalProjectsAffected = existingRequestsList.Count
                };

                return ApiResponse<DocumentRequirementResponseDto>.Success(response, 
                    $"Document requirement updated successfully for {existingRequestsList.Count} projects in batch {request.BatchId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating document requirement for batch {BatchId}", request.BatchId);
                return ApiResponse<DocumentRequirementResponseDto>.Fail($"Failed to update document requirement: {ex.Message}");
            }
        }
    }
}