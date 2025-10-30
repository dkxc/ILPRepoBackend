using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrappers;
using MediatR;

namespace IlpRepoBackend.Application.Query
{
    public class GetDocumentLinkQuery : IRequest<ApiResponse<DocumentLinkDto>>
    {
        public int DocumentId { get; set; }

        public GetDocumentLinkQuery(int documentId)
        {
            DocumentId = documentId;
        }
    }
}