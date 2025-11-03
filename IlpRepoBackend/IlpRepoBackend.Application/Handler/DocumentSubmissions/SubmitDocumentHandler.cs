using IlpRepoBackend.Application.Command.DocumentSubmissions;
using IlpRepoBackend.Application.Dto.DocumentSubmissions;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using IlpRepoBackend.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IlpRepoBackend.Application.Handler.DocumentSubmissions
{
    public class SubmitDocumentHandler : IRequestHandler<SubmitDocumentCommand, ApiResponse<DocumentSubmissionResultDto>>
    {
        private readonly IDocumentSubmissionRepository _documentSubmissionRepository;
        private readonly IDocumentRequestRepository _documentRequestRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<SubmitDocumentHandler> _logger;

        public SubmitDocumentHandler(
            IDocumentSubmissionRepository documentSubmissionRepository,
            IDocumentRequestRepository documentRequestRepository,
            IFileStorageService fileStorageService,
            ILogger<SubmitDocumentHandler> logger)
        {
            _documentSubmissionRepository = documentSubmissionRepository;
            _documentRequestRepository = documentRequestRepository;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        public async Task<ApiResponse<DocumentSubmissionResultDto>> Handle(SubmitDocumentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Get document request with includes
                var documentRequest = await _documentRequestRepository.GetByIdWithIncludesAsync(request.DocumentRequestId);

                if (documentRequest == null)
                {
                    return ApiResponse<DocumentSubmissionResultDto>.Fail($"Document request with ID {request.DocumentRequestId} not found");
                }

                // Validate file
                if (request.DocumentFile == null || request.DocumentFile.Length == 0)
                {
                    return ApiResponse<DocumentSubmissionResultDto>.Fail("Document file is required");
                }

                // Upload file to storage
                string submissionLink;
                using (var stream = request.DocumentFile.OpenReadStream())
                {
                    submissionLink = await _fileStorageService.UploadFileAsync(
                        stream,
                        request.DocumentFile.FileName,
                        request.DocumentFile.ContentType);
                }

                // Get file extension
                var fileExtension = Path.GetExtension(request.DocumentFile.FileName)?.TrimStart('.') ?? "unknown";

                // Create document submission
                var submission = new DocumentSubmission
                {
                    DocumentId = documentRequest.DocumentId,
                    RequestId = documentRequest.Id,
                    FileName = request.DocumentFile.FileName,
                    FileType = fileExtension.ToUpperInvariant(),
                    SubmissionLink = submissionLink,
                    SubmissionDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var savedSubmission = await _documentSubmissionRepository.AddAsync(submission);

                var isLate = savedSubmission.SubmissionDate > documentRequest.DueDate;

                var response = new DocumentSubmissionResultDto
                {
                    SubmissionId = savedSubmission.Id,
                    FileName = savedSubmission.FileName ?? "Unknown",
                    FileType = savedSubmission.FileType ?? "Unknown",
                    SubmissionLink = savedSubmission.SubmissionLink ?? string.Empty,
                    SubmissionDate = savedSubmission.SubmissionDate,
                    DocumentTypeName = documentRequest.Document?.Name ?? "Unknown",
                    ProjectName = documentRequest.Project?.ProjectName ?? "Unknown",
                    IsLateSubmission = isLate
                };

                _logger.LogInformation($"Document submitted successfully for request {request.DocumentRequestId}");

                return ApiResponse<DocumentSubmissionResultDto>.Success(
                    response,
                    "Document submitted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting document for request {DocumentRequestId}", request.DocumentRequestId);
                return ApiResponse<DocumentSubmissionResultDto>.Fail($"Failed to submit document: {ex.Message}");
            }
        }
    }
}