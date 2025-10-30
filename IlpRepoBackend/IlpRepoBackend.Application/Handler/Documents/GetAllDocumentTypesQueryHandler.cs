using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Query;
using IlpRepoBackend.Application.Wrappers;
using IlpRepoBackend.Domain.Persistence;
using MediatR;

namespace IlpRepoBackend.Application.Handler.Documents
{
    public class GetAllDocumentTypesQueryHandler : IRequestHandler<GetAllDocumentTypesQuery, ApiResponse<List<DocumentTypeDto>>>
    {
        private readonly IDocumentRepository _documentRepository;

        public GetAllDocumentTypesQueryHandler(IDocumentRepository documentRepository)
        {
            _documentRepository = documentRepository;
        }

        public async Task<ApiResponse<List<DocumentTypeDto>>> Handle(GetAllDocumentTypesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var documents = await _documentRepository.GetAllDocumentTypesAsync();

                var documentTypeDtos = documents.Select(d => new DocumentTypeDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Type = d.Type
                }).ToList();

                return new ApiResponse<List<DocumentTypeDto>>(documentTypeDtos, "Document types retrieved successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<DocumentTypeDto>>($"An error occurred: {ex.Message}", 500);
            }
        }
    }
}