using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrappers;
using MediatR;

namespace IlpRepoBackend.Application.Query
{
    public class GetAllDocumentTypesQuery : IRequest<ApiResponse<List<DocumentTypeDto>>>
    {
        public GetAllDocumentTypesQuery()
        {
        }
    }
}