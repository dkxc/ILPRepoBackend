using IlpRepoBackend.Application.Dto.Links;
using IlpRepoBackend.Application.Wrapper;
using MediatR;
using System.Collections.Generic;

namespace IlpRepoBackend.Application.Query.Links
{
    public class GetAllLinkTypesQuery : IRequest<ApiResponse<List<LinkTypeDto>>>
    {
    }
}