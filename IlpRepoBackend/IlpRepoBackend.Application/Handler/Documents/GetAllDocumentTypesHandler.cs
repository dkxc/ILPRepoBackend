using IlpRepoBackend.Application.Dto.Documents;
using IlpRepoBackend.Application.Query.Documents;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IlpRepoBackend.Application.Handler.Documents
{
    public class GetAllDocumentTypesHandler : IRequestHandler<GetAllDocumentTypesQuery, ApiResponse<List<DocumentTypeDto>>>
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly ILogger<GetAllDocumentTypesHandler> _logger;

        public GetAllDocumentTypesHandler(
            IDocumentRepository documentRepository,
            ILogger<GetAllDocumentTypesHandler> logger)
        {
            _documentRepository = documentRepository;
            _logger = logger;
        }

        public async Task<ApiResponse<List<DocumentTypeDto>>> Handle(GetAllDocumentTypesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var documents = await _documentRepository.GetAllAsync();

                var documentDtos = documents.Select(d => new DocumentTypeDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Link = d.Link,
                    UploadDate = d.UploadDate,
                    CreatedAt = d.CreatedAt,
                    UpdatedAt = d.UpdatedAt
                }).OrderBy(d => d.Name).ToList();

                _logger.LogInformation($"Retrieved {documentDtos.Count} document types");
                return ApiResponse<List<DocumentTypeDto>>.Success(documentDtos, "Document types retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document types");
                return ApiResponse<List<DocumentTypeDto>>.Fail($"Failed to retrieve document types: {ex.Message}");
            }
        }
    }
}