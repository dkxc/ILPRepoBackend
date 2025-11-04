using IlpRepoBackend.Application.Command.Documents;
using IlpRepoBackend.Application.Dto.Documents;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using IlpRepoBackend.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IlpRepoBackend.Application.Handler.Documents
{
    public class CreateDocumentTypeHandler : IRequestHandler<CreateDocumentTypeCommand, ApiResponse<CreatedDocumentTypeDto>>
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IFileStorageService _storageService;
        private readonly ILogger<CreateDocumentTypeHandler> _logger;

        public CreateDocumentTypeHandler(
            IDocumentRepository documentRepository,
            IFileStorageService storageService,
            ILogger<CreateDocumentTypeHandler> logger)
        {
            _documentRepository = documentRepository;
            _storageService = storageService;
            _logger = logger;
        }

        public async Task<ApiResponse<CreatedDocumentTypeDto>> Handle(CreateDocumentTypeCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if document type with this name already exists
                var existingDocument = await _documentRepository.FirstOrDefaultAsync(d => d.Name.ToLower() == request.Name.ToLower());
                if (existingDocument != null)
                {
                    return ApiResponse<CreatedDocumentTypeDto>.Fail($"Document type with name '{request.Name}' already exists");
                }
                
                string? fileUrl = null;

                // Handle file upload if provided
                if (request.TemplateFile != null)
                {
                    // Validate file
                    if (request.TemplateFile.Length == 0)
                    {
                        return ApiResponse<CreatedDocumentTypeDto>.Fail("Template file is empty");
                    }

                    // Upload file to Supabase
                    using var stream = request.TemplateFile.OpenReadStream();
                    fileUrl = await _storageService.UploadFileAsync(
                        stream, 
                        request.TemplateFile.FileName, 
                        request.TemplateFile.ContentType);
                }

                // Create new document
                var document = new Domain.Entities.Documents
                {
                    Name = request.Name,
                    FileType = request.FileType,
                    Link = fileUrl,
                    UploadDate = fileUrl != null ? DateTime.UtcNow : DateTime.MinValue,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                document = await _documentRepository.AddAsync(document);
                
                _logger.LogInformation($"Created new document type: {request.Name}");

                var response = new CreatedDocumentTypeDto
                {
                    Id = document.Id,
                    Name = document.Name,
                    FileType = document.FileType,
                    Link = document.Link,
                    CreatedAt = document.CreatedAt
                };

                return ApiResponse<CreatedDocumentTypeDto>.Success(response, "Document type created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating document type: {Name}", request.Name);
                return ApiResponse<CreatedDocumentTypeDto>.Fail($"Failed to create document type: {ex.Message}");
            }
        }
    }
}