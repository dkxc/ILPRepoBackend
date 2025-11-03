using IlpRepoBackend.Application.Command.DocumentRequests;
using IlpRepoBackend.Application.Dto.DocumentRequests;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IlpRepoBackend.Application.Handler.DocumentRequests
{
    public class SetDocumentRequirementHandler : IRequestHandler<SetDocumentRequirementCommand, ApiResponse<DocumentRequirementResponseDto>>
    {
        private readonly IDocumentRequestRepository _documentRequestRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IDocumentRepository _documentRepository;
        private readonly ILogger<SetDocumentRequirementHandler> _logger;

        public SetDocumentRequirementHandler(
            IDocumentRequestRepository documentRequestRepository,
            IProjectRepository projectRepository,
            IDocumentRepository documentRepository,
            ILogger<SetDocumentRequirementHandler> logger)
        {
            _documentRequestRepository = documentRequestRepository;
            _projectRepository = projectRepository;
            _documentRepository = documentRepository;
            _logger = logger;
        }

        public async Task<ApiResponse<DocumentRequirementResponseDto>> Handle(SetDocumentRequirementCommand request, CancellationToken cancellationToken)
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

                // Check if document requirement already exists for any project in this batch
                var existingRequests = await _documentRequestRepository.GetByDocumentIdAndProjectIdsAsync(request.DocumentTypeId, projectIds);
                if (existingRequests.Any())
                {
                    return ApiResponse<DocumentRequirementResponseDto>.Fail($"Document requirement for '{documentType.Name}' already exists for batch {request.BatchId}");
                }

                // Create document requests for all projects in the batch
                var documentRequests = new List<DocumentRequest>();
                foreach (var project in projectsList)
                {
                    var documentRequest = new DocumentRequest
                    {
                        ProjectId = project.Id,
                        DocumentId = request.DocumentTypeId,
                        RequestDate = DateTime.UtcNow,
                        DueDate = request.DueDate,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    documentRequests.Add(await _documentRequestRepository.AddAsync(documentRequest));
                }

                _logger.LogInformation($"Set document requirement '{documentType.Name}' for batch {request.BatchId} affecting {projectsList.Count} projects");

                var response = new DocumentRequirementResponseDto
                {
                    DocumentTypeId = request.DocumentTypeId,
                    DocumentTypeName = documentType.Name,
                    BatchId = request.BatchId,
                    DueDate = request.DueDate,
                    RequestDate = DateTime.UtcNow,
                    ProjectIds = projectIds,
                    TotalProjectsAffected = projectsList.Count
                };

                return ApiResponse<DocumentRequirementResponseDto>.Success(response, 
                    $"Document requirement set successfully for {projectsList.Count} projects in batch {request.BatchId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting document requirement for batch {BatchId}", request.BatchId);
                return ApiResponse<DocumentRequirementResponseDto>.Fail($"Failed to set document requirement: {ex.Message}");
            }
        }
    }
}