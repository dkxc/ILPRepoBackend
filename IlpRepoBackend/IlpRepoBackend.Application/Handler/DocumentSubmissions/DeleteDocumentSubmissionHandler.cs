using IlpRepoBackend.Application.Command.DocumentSubmissions;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Persistence;
using IlpRepoBackend.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IlpRepoBackend.Application.Handler.DocumentSubmissions
{
    public class DeleteDocumentSubmissionHandler : IRequestHandler<DeleteDocumentSubmissionCommand, ApiResponse<bool>>
    {
        private readonly IDocumentSubmissionRepository _documentSubmissionRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<DeleteDocumentSubmissionHandler> _logger;

        public DeleteDocumentSubmissionHandler(
            IDocumentSubmissionRepository documentSubmissionRepository,
            IFileStorageService fileStorageService,
            ILogger<DeleteDocumentSubmissionHandler> logger)
        {
            _documentSubmissionRepository = documentSubmissionRepository;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        public async Task<ApiResponse<bool>> Handle(DeleteDocumentSubmissionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Get the submission to delete
                var submission = await _documentSubmissionRepository.GetByIdAsync(request.SubmissionId);
                
                if (submission == null)
                {
                    return ApiResponse<bool>.Fail($"Document submission with ID {request.SubmissionId} not found");
                }

                // Try to delete file from storage if it exists
                if (!string.IsNullOrEmpty(submission.SubmissionLink))
                {
                    try
                    {
                        // Extract filename from the URL
                        var uri = new Uri(submission.SubmissionLink);
                        var fileName = Path.GetFileName(uri.LocalPath);
                        
                        await _fileStorageService.DeleteFileAsync(fileName);
                        _logger.LogInformation($"Successfully deleted file {fileName} from storage");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete file from storage for submission {SubmissionId}", request.SubmissionId);
                        // Continue with database deletion even if file deletion fails
                    }
                }

                // Delete from database
                var success = await _documentSubmissionRepository.DeleteAsync(request.SubmissionId);
                
                if (!success)
                {
                    return ApiResponse<bool>.Fail($"Failed to delete document submission with ID {request.SubmissionId}");
                }

                _logger.LogInformation($"Document submission {request.SubmissionId} deleted successfully");

                return ApiResponse<bool>.Success(true, "Document submission deleted successfully. You can now resubmit.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document submission {SubmissionId}", request.SubmissionId);
                return ApiResponse<bool>.Fail($"Failed to delete document submission: {ex.Message}");
            }
        }
    }
}