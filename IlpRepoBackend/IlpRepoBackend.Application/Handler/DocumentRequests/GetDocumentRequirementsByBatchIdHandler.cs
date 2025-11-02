using IlpRepoBackend.Application.Dto.DocumentRequests;
using IlpRepoBackend.Application.Query.DocumentRequests;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IlpRepoBackend.Application.Handler.DocumentRequests
{
    public class GetDocumentRequirementsByBatchIdHandler : IRequestHandler<GetDocumentRequirementsByBatchIdQuery, ApiResponse<List<DocumentRequirementResponseDto>>>
    {
        private readonly IDocumentRequestRepository _documentRequestRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IDocumentRepository _documentRepository;
        private readonly ILogger<GetDocumentRequirementsByBatchIdHandler> _logger;

        public GetDocumentRequirementsByBatchIdHandler(
            IDocumentRequestRepository documentRequestRepository,
            IProjectRepository projectRepository,
            IDocumentRepository documentRepository,
            ILogger<GetDocumentRequirementsByBatchIdHandler> logger)
        {
            _documentRequestRepository = documentRequestRepository;
            _projectRepository = projectRepository;
            _documentRepository = documentRepository;
            _logger = logger;
        }

        public async Task<ApiResponse<List<DocumentRequirementResponseDto>>> Handle(GetDocumentRequirementsByBatchIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Get all projects for the given batch
                var projects = await _projectRepository.GetProjectsByBatchIdAsync(request.BatchId);
                var projectsList = projects.ToList();

                if (!projectsList.Any())
                {
                    _logger.LogInformation($"No projects found for batch ID {request.BatchId}");
                    return ApiResponse<List<DocumentRequirementResponseDto>>.Success(
                        new List<DocumentRequirementResponseDto>(), 
                        "No projects found for this batch");
                }

                var projectIds = projectsList.Select(p => p.Id).ToList();

                // Get all document requests for these projects
                var allDocumentRequests = new List<Domain.Entities.DocumentRequest>();
                foreach (var projectId in projectIds)
                {
                    var projectRequests = await _documentRequestRepository.GetByProjectIdAsync(projectId);
                    allDocumentRequests.AddRange(projectRequests);
                }

                // Group by document type to get batch-level requirements
                var groupedRequests = allDocumentRequests
                    .Where(dr => dr.DocumentId.HasValue)
                    .GroupBy(dr => dr.DocumentId.Value)
                    .ToList();

                var requirements = new List<DocumentRequirementResponseDto>();

                foreach (var group in groupedRequests)
                {
                    var firstRequest = group.First();
                    var documentType = await _documentRepository.GetByIdAsync(group.Key);
                    
                    if (documentType != null)
                    {
                        var projectIdsForThisDocument = group.Select(dr => dr.ProjectId ?? 0).Where(id => id > 0).ToList();
                        
                        requirements.Add(new DocumentRequirementResponseDto
                        {
                            DocumentTypeId = group.Key,
                            DocumentTypeName = documentType.Name,
                            BatchId = request.BatchId,
                            DueDate = firstRequest.DueDate, // Assuming all have same due date
                            RequestDate = firstRequest.RequestDate,
                            ProjectIds = projectIdsForThisDocument,
                            TotalProjectsAffected = projectIdsForThisDocument.Count
                        });
                    }
                }

                _logger.LogInformation($"Retrieved {requirements.Count} document requirements for batch {request.BatchId}");

                return ApiResponse<List<DocumentRequirementResponseDto>>.Success(
                    requirements.OrderBy(r => r.DocumentTypeName).ToList(),
                    $"Retrieved {requirements.Count} document requirements for batch {request.BatchId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document requirements for batch {BatchId}", request.BatchId);
                return ApiResponse<List<DocumentRequirementResponseDto>>.Fail($"Failed to retrieve document requirements: {ex.Message}");
            }
        }
    }
}