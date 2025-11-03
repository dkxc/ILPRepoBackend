using IlpRepoBackend.Application.Dto.DocumentSubmissions;
using IlpRepoBackend.Application.Query.DocumentSubmissions;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IlpRepoBackend.Application.Handler.DocumentSubmissions
{
    public class GetDocumentSubmissionsByProjectIdHandler : IRequestHandler<GetDocumentSubmissionsByProjectIdQuery, ApiResponse<List<ProjectDocumentSubmissionInfo>>>
    {
        private readonly IDocumentSubmissionRepository _documentSubmissionRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ILogger<GetDocumentSubmissionsByProjectIdHandler> _logger;

        public GetDocumentSubmissionsByProjectIdHandler(
            IDocumentSubmissionRepository documentSubmissionRepository,
            IProjectRepository projectRepository,
            ILogger<GetDocumentSubmissionsByProjectIdHandler> logger)
        {
            _documentSubmissionRepository = documentSubmissionRepository;
            _projectRepository = projectRepository;
            _logger = logger;
        }

        public async Task<ApiResponse<List<ProjectDocumentSubmissionInfo>>> Handle(GetDocumentSubmissionsByProjectIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var project = await _projectRepository.GetByIdAsync(request.ProjectId);
                if (project == null)
                {
                    return ApiResponse<List<ProjectDocumentSubmissionInfo>>.Fail($"Project with ID {request.ProjectId} not found");
                }

                var submissions = await _documentSubmissionRepository.GetByProjectIdAsync(request.ProjectId);
                var submissionsList = submissions.ToList();

                var submissionDtos = new List<ProjectDocumentSubmissionInfo>();

                foreach (var submission in submissionsList)
                {
                    if (submission.Document != null && submission.DocumentRequest != null)
                    {
                        submissionDtos.Add(new ProjectDocumentSubmissionInfo
                        {
                            Id = submission.Id,
                            FileName = submission.FileName ?? "Unknown",
                            FileType = submission.FileType ?? "Unknown",
                            SubmissionLink = submission.SubmissionLink,
                            SubmissionDate = submission.SubmissionDate,
                            DocumentTypeId = submission.Document.Id,
                            DocumentTypeName = submission.Document.Name,
                            ProjectId = request.ProjectId,
                            ProjectName = project.ProjectName,
                            DueDate = submission.DocumentRequest.DueDate
                        });
                    }
                }

                _logger.LogInformation($"Retrieved {submissionDtos.Count} document submissions for project {request.ProjectId}");

                return ApiResponse<List<ProjectDocumentSubmissionInfo>>.Success(
                    submissionDtos,
                    $"Retrieved {submissionDtos.Count} document submissions for project '{project.ProjectName}'");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document submissions for project {ProjectId}", request.ProjectId);
                return ApiResponse<List<ProjectDocumentSubmissionInfo>>.Fail($"Failed to retrieve document submissions: {ex.Message}");
            }
        }
    }
}