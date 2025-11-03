using IlpRepoBackend.Application.Dto.DocumentRequests;
using IlpRepoBackend.Application.Query.DocumentRequests;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IlpRepoBackend.Application.Handler.DocumentRequests
{
    public class GetDocumentRequirementsByProjectIdHandler : IRequestHandler<GetDocumentRequirementsByProjectIdQuery, ApiResponse<List<ProjectDocumentRequirementDto>>>
    {
        private readonly IDocumentRequestRepository _documentRequestRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IDocumentRepository _documentRepository;
        private readonly ILogger<GetDocumentRequirementsByProjectIdHandler> _logger;

        public GetDocumentRequirementsByProjectIdHandler(
            IDocumentRequestRepository documentRequestRepository,
            IProjectRepository projectRepository,
            IDocumentRepository documentRepository,
            ILogger<GetDocumentRequirementsByProjectIdHandler> logger)
        {
            _documentRequestRepository = documentRequestRepository;
            _projectRepository = projectRepository;
            _documentRepository = documentRepository;
            _logger = logger;
        }

        public async Task<ApiResponse<List<ProjectDocumentRequirementDto>>> Handle(GetDocumentRequirementsByProjectIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Validate project exists
                var project = await _projectRepository.GetByIdAsync(request.ProjectId);
                if (project == null)
                {
                    return ApiResponse<List<ProjectDocumentRequirementDto>>.Fail($"Project with ID {request.ProjectId} not found");
                }

                // Get all document requests for this project
                var documentRequests = await _documentRequestRepository.GetByProjectIdAsync(request.ProjectId);
                var documentRequestsList = documentRequests.ToList();

                if (!documentRequestsList.Any())
                {
                    _logger.LogInformation($"No document requirements found for project ID {request.ProjectId}");
                    return ApiResponse<List<ProjectDocumentRequirementDto>>.Success(
                        new List<ProjectDocumentRequirementDto>(), 
                        "No document requirements found for this project");
                }

                var projectRequirements = new List<ProjectDocumentRequirementDto>();

                foreach (var docRequest in documentRequestsList)
                {
                    if (docRequest.DocumentId.HasValue)
                    {
                        var documentType = await _documentRepository.GetByIdAsync(docRequest.DocumentId.Value);
                        
                        if (documentType != null)
                        {
                            // Check if there are any submissions for this document request
                            var hasSubmission = docRequest.DocumentSubmissions?.Any() ?? false;
                            var lastSubmissionDate = docRequest.DocumentSubmissions?
                                .OrderByDescending(ds => ds.SubmissionDate)
                                .FirstOrDefault()?.SubmissionDate;

                            projectRequirements.Add(new ProjectDocumentRequirementDto
                            {
                                DocumentRequestId = docRequest.Id,
                                DocumentTypeId = docRequest.DocumentId.Value,
                                DocumentTypeName = documentType.Name,
                                DocumentTemplateUrl = documentType.Link,
                                ProjectId = request.ProjectId,
                                ProjectName = project.ProjectName,
                                DueDate = docRequest.DueDate,
                                RequestDate = docRequest.RequestDate,
                                HasSubmission = hasSubmission,
                                LastSubmissionDate = lastSubmissionDate
                            });
                        }
                    }
                }

                // Sort by urgency (overdue first, then by due date)
                var sortedRequirements = projectRequirements
                    .OrderBy(req => req.IsOverdue ? 0 : 1)
                    .ThenBy(req => req.DueDate)
                    .ToList();

                _logger.LogInformation($"Retrieved {sortedRequirements.Count} document requirements for project {request.ProjectId}");

                return ApiResponse<List<ProjectDocumentRequirementDto>>.Success(
                    sortedRequirements,
                    $"Retrieved {sortedRequirements.Count} document requirements for project '{project.ProjectName}'");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document requirements for project {ProjectId}", request.ProjectId);
                return ApiResponse<List<ProjectDocumentRequirementDto>>.Fail($"Failed to retrieve document requirements: {ex.Message}");
            }
        }
    }
}