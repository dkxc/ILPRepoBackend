using IlpRepoBackend.Application.Command;
using IlpRepoBackend.Application.CustomException;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrappers;
using IlpRepoBackend.Domain.Persistence;
using MediatR;

namespace IlpRepoBackend.Application.Handler.DocumentRequests
{
    public class UpdateDocumentRequestCommandHandler : IRequestHandler<UpdateDocumentRequestCommand, ApiResponse<DocumentRequestResponseDto>>
    {
        private readonly IDocumentRequestRepository _documentRequestRepository;

        public UpdateDocumentRequestCommandHandler(IDocumentRequestRepository documentRequestRepository)
        {
            _documentRequestRepository = documentRequestRepository;
        }

        public async Task<ApiResponse<DocumentRequestResponseDto>> Handle(UpdateDocumentRequestCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Get existing document request
                var documentRequest = await _documentRequestRepository.GetDocumentRequestWithDetailsAsync(request.Id);
                if (documentRequest == null)
                {
                    throw new NotFoundException("Document Request", request.Id);
                }

                // Store old file URL for response
                var oldFileUrl = documentRequest.FileUrl;

                // Validate document exists
                var document = await _documentRequestRepository.GetDocumentByIdAsync(request.DocumentId);
                if (document == null)
                {
                    throw new NotFoundException("Document", request.DocumentId);
                }

                // Get project ID from batch ID
                var projectId = await _documentRequestRepository.GetProjectIdByBatchIdAsync(request.BatchId);
                if (projectId == null)
                {
                    return new ApiResponse<DocumentRequestResponseDto>($"No project found for batch ID {request.BatchId}", 404);
                }

                // Validate project exists
                var project = await _documentRequestRepository.GetProjectByIdAsync(projectId.Value);
                if (project == null)
                {
                    throw new NotFoundException("Project", projectId.Value);
                }

                // Validate due date
                if (request.DueDate <= DateTime.UtcNow)
                {
                    return new ApiResponse<DocumentRequestResponseDto>("Due date must be in the future", 400);
                }

                // Update document request properties
                documentRequest.ProjectId = projectId.Value;
                documentRequest.DocumentId = request.DocumentId;
                documentRequest.DueDate = request.DueDate;
                documentRequest.UpdatedAt = DateTime.UtcNow;

                // Handle file URL updates
                if (request.RemoveExistingFile)
                {
                    documentRequest.FileUrl = null;
                }
                else if (!string.IsNullOrEmpty(request.FileUrl))
                {
                    documentRequest.FileUrl = request.FileUrl;
                }
                // else keep existing file URL

                // Update the document request
                await _documentRequestRepository.UpdateAsync(documentRequest);

                // Create response DTO
                var responseDto = new DocumentRequestResponseDto
                {
                    Id = documentRequest.Id,
                    ProjectId = projectId.Value,
                    ProjectName = project.ProjectName,
                    DocumentId = request.DocumentId,
                    DocumentName = document.Name,
                    RequestDate = documentRequest.RequestDate,
                    DueDate = documentRequest.DueDate,
                    FileUrl = documentRequest.FileUrl,
                    CreatedAt = documentRequest.CreatedAt
                };

                return new ApiResponse<DocumentRequestResponseDto>(responseDto, "Document request updated successfully");
            }
            catch (NotFoundException ex)
            {
                return new ApiResponse<DocumentRequestResponseDto>(ex.Message, 404);
            }
            catch (Exception ex)
            {
                return new ApiResponse<DocumentRequestResponseDto>($"An error occurred while updating document request: {ex.Message}", 500);
            }
        }
    }
}