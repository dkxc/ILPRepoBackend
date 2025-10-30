using IlpRepoBackend.Application.Command;
using IlpRepoBackend.Application.CustomException;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrappers;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using MediatR;

namespace IlpRepoBackend.Application.Handler.DocumentRequests
{
    public class CreateDocumentRequestCommandHandler : IRequestHandler<CreateDocumentRequestCommand, ApiResponse<DocumentRequestResponseDto>>
    {
        private readonly IDocumentRequestRepository _documentRequestRepository;

        public CreateDocumentRequestCommandHandler(IDocumentRequestRepository documentRequestRepository)
        {
            _documentRequestRepository = documentRequestRepository;
        }

        public async Task<ApiResponse<DocumentRequestResponseDto>> Handle(CreateDocumentRequestCommand request, CancellationToken cancellationToken)
        {
            try
            {
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

                // Create document request
                var documentRequest = new DocumentRequest
                {
                    ProjectId = projectId.Value,
                    DocumentId = request.DocumentId,
                    RequestDate = DateTime.UtcNow,
                    DueDate = request.DueDate,
                    FileUrl = request.FileUrl,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdRequest = await _documentRequestRepository.AddAsync(documentRequest);

                // Create response DTO
                var responseDto = new DocumentRequestResponseDto
                {
                    Id = createdRequest.Id,
                    ProjectId = projectId.Value,
                    ProjectName = project.ProjectName,
                    DocumentId = request.DocumentId,
                    DocumentName = document.Name,
                    RequestDate = createdRequest.RequestDate,
                    DueDate = createdRequest.DueDate,
                    FileUrl = createdRequest.FileUrl,
                    CreatedAt = createdRequest.CreatedAt
                };

                return new ApiResponse<DocumentRequestResponseDto>(responseDto, "Document request created successfully");
            }
            catch (NotFoundException ex)
            {
                return new ApiResponse<DocumentRequestResponseDto>(ex.Message, 404);
            }
            catch (Exception ex)
            {
                return new ApiResponse<DocumentRequestResponseDto>($"An error occurred while creating document request: {ex.Message}", 500);
            }
        }
    }
}