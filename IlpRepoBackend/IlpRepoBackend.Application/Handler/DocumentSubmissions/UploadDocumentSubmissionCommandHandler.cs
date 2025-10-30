using IlpRepoBackend.Application.Command;
using IlpRepoBackend.Application.CustomException;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Services;
using IlpRepoBackend.Application.Wrappers;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using MediatR;

namespace IlpRepoBackend.Application.Handler.DocumentSubmissions
{
    public class UploadDocumentSubmissionCommandHandler : IRequestHandler<UploadDocumentSubmissionCommand, ApiResponse<DocumentSubmissionDto>>
    {
        private readonly IDocumentSubmissionRepository _documentSubmissionRepository;
        private readonly IDocumentRequestRepository _documentRequestRepository;
        private readonly IDocumentRepository _documentRepository;
        private readonly IProjectAuthorizationService _authorizationService;

        public UploadDocumentSubmissionCommandHandler(
            IDocumentSubmissionRepository documentSubmissionRepository,
            IDocumentRequestRepository documentRequestRepository,
            IDocumentRepository documentRepository,
            IProjectAuthorizationService authorizationService)
        {
            _documentSubmissionRepository = documentSubmissionRepository;
            _documentRequestRepository = documentRequestRepository;
            _documentRepository = documentRepository;
            _authorizationService = authorizationService;
        }

        public async Task<ApiResponse<DocumentSubmissionDto>> Handle(UploadDocumentSubmissionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Validate document request exists
                var documentRequest = await _documentRequestRepository.GetDocumentRequestWithDetailsAsync(request.RequestId);
                if (documentRequest == null)
                {
                    throw new NotFoundException("Document Request", request.RequestId);
                }

                // Get ProjectId from DocumentRequest
                var projectId = documentRequest.ProjectId;
                if (!projectId.HasValue)
                {
                    return new ApiResponse<DocumentSubmissionDto>("Document request does not have an associated project", 400);
                }

                // AUTHORIZATION: Check if trainee is Team Leader of this project
                var isTeamLeader = await _authorizationService.IsTeamLeaderAsync(request.TraineeId, projectId.Value);
                if (!isTeamLeader)
                {
                    return new ApiResponse<DocumentSubmissionDto>(
                        "Unauthorized: Only the Team Leader can submit documents for this project", 
                        403);
                }

                // Validate document exists
                var document = await _documentRepository.GetDocumentWithLinkAsync(request.DocumentId);
                if (document == null)
                {
                    throw new NotFoundException("Document", request.DocumentId);
                }

                // Check if document IDs match
                if (documentRequest.DocumentId != request.DocumentId)
                {
                    return new ApiResponse<DocumentSubmissionDto>("Document ID does not match the document request", 400);
                }

                // Check if submission is past due date
                if (DateTime.UtcNow > documentRequest.DueDate)
                {
                    return new ApiResponse<DocumentSubmissionDto>($"Submission deadline has passed. Due date was {documentRequest.DueDate:yyyy-MM-dd}", 400);
                }

                // Create document submission
                var submission = new DocumentSubmission
                {
                    RequestId = request.RequestId,
                    DocumentId = request.DocumentId,
                    ProjectId = projectId,
                    TraineeId = request.TraineeId,
                    SubmissionLink = request.SubmissionLink,
                    FileName = request.FileName,
                    FileType = request.FileType,
                    SubmissionDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdSubmission = await _documentSubmissionRepository.AddAsync(submission);

                // Create response DTO
                var submissionDto = new DocumentSubmissionDto
                {
                    Id = createdSubmission.Id,
                    SubmissionLink = createdSubmission.SubmissionLink,
                    FileName = createdSubmission.FileName,
                    FileType = createdSubmission.FileType,
                    DocumentName = document.Name,
                    ProjectId = projectId,
                    ProjectName = documentRequest.Project?.ProjectName,
                    TraineeId = request.TraineeId,
                    SubmissionDate = createdSubmission.SubmissionDate
                };

                return new ApiResponse<DocumentSubmissionDto>(submissionDto, "Document submitted successfully");
            }
            catch (NotFoundException ex)
            {
                return new ApiResponse<DocumentSubmissionDto>(ex.Message, 404);
            }
            catch (Exception ex)
            {
                return new ApiResponse<DocumentSubmissionDto>($"An error occurred while uploading document: {ex.Message}", 500);
            }
        }
    }
}