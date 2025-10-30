using IlpRepoBackend.Application.CustomException;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Query;
using IlpRepoBackend.Application.Wrappers;
using IlpRepoBackend.Domain.Persistence;
using MediatR;

namespace IlpRepoBackend.Application.Handler.Documents
{
    public class GetDocumentLinkQueryHandler : IRequestHandler<GetDocumentLinkQuery, ApiResponse<DocumentLinkDto>>
    {
        private readonly IDocumentRepository _documentRepository;

        public GetDocumentLinkQueryHandler(IDocumentRepository documentRepository)
        {
            _documentRepository = documentRepository;
        }

        public async Task<ApiResponse<DocumentLinkDto>> Handle(GetDocumentLinkQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var document = await _documentRepository.GetDocumentWithLinkAsync(request.DocumentId);

                if (document == null)
                {
                    throw new NotFoundException("Document", request.DocumentId);
                }

                var documentLinkDto = new DocumentLinkDto
                {
                    Id = document.Id,
                    Name = document.Name,
                    Type = document.Type,
                    TemplateLink = document.TemplateLink
                };

                return new ApiResponse<DocumentLinkDto>(documentLinkDto, "Document template link retrieved successfully");
            }
            catch (NotFoundException ex)
            {
                return new ApiResponse<DocumentLinkDto>(ex.Message, 404);
            }
            catch (Exception ex)
            {
                return new ApiResponse<DocumentLinkDto>($"An error occurred: {ex.Message}", 500);
            }
        }
    }
}