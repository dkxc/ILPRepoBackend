using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrappers;
using MediatR;

namespace IlpRepoBackend.Application.Query
{
    public class GetAllLinkTypesQuery : IRequest<ApiResponse<List<LinkTypeDto>>>
    {
        public GetAllLinkTypesQuery()
        {
        }
    }
}