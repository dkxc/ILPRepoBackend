using IlpRepoBackend.Application.Dto.Documents;
using IlpRepoBackend.Application.Wrapper;
using MediatR;

namespace IlpRepoBackend.Application.Query.Documents
{
    public class GetAllDocumentTypesQuery : IRequest<ApiResponse<List<DocumentTypeDto>>>
    {
    }
}