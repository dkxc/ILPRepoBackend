using IlpRepoBackend.Application.Command.Documents;
using IlpRepoBackend.Application.Dto.Documents;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Persistence;
using IlpRepoBackend.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IlpRepoBackend.Application.Handler.Documents
{
    public class UpdateDocumentTypeHandler : IRequestHandler<UpdateDocumentTypeCommand, ApiResponse<DocumentTypeDto>>
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IFileStorageService _storageService;
        private readonly ILogger<UpdateDocumentTypeHandler> _logger;

        public UpdateDocumentTypeHandler(
            IDocumentRepository documentRepository,
            IFileStorageService storageService,
            ILogger<UpdateDocumentTypeHandler> logger)
        {
            _documentRepository = documentRepository;
            _storageService = storageService;
            _logger = logger;
        }

        public async Task<ApiResponse<DocumentTypeDto>> Handle(UpdateDocumentTypeCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var document = await _documentRepository.GetByIdAsync(request.Id);
                if (document == null)
                {
                    return ApiResponse<DocumentTypeDto>.Fail($"Document type with ID {request.Id} not found");
                }

                // Update name
                document.Name = request.Name;

                // Handle file upload if provided
                if (request.TemplateFile != null)
                {
                    if (request.TemplateFile.Length == 0)
                    {
                        return ApiResponse<DocumentTypeDto>.Fail("Template file is empty");
                    }

                    // Upload new file to Supabase
                    using var stream = request.TemplateFile.OpenReadStream();
                    var newFileUrl = await _storageService.UploadFileAsync(
                        stream, 
                        request.TemplateFile.FileName, 
                        request.TemplateFile.ContentType);

                    document.Link = newFileUrl;
                    document.UploadDate = DateTime.UtcNow;
                }

                document.UpdatedAt = DateTime.UtcNow;
                var updatedDocument = await _documentRepository.UpdateAsync(document);

                var response = new DocumentTypeDto
                {
                    Id = updatedDocument.Id,
                    Name = updatedDocument.Name,
                    Link = updatedDocument.Link,
                    UploadDate = updatedDocument.UploadDate,
                    CreatedAt = updatedDocument.CreatedAt,
                    UpdatedAt = updatedDocument.UpdatedAt
                };

                _logger.LogInformation($"Updated document type: {request.Name}");
                return ApiResponse<DocumentTypeDto>.Success(response, "Document type updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating document type: {Id}", request.Id);
                return ApiResponse<DocumentTypeDto>.Fail($"Failed to update document type: {ex.Message}");
            }
        }
    }
}