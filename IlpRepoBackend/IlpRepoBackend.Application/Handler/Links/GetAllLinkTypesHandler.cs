using IlpRepoBackend.Application.Dto.Links;
using IlpRepoBackend.Application.Query.Links;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handler.Links
{
    public class GetAllLinkTypesHandler : IRequestHandler<GetAllLinkTypesQuery, ApiResponse<List<LinkTypeDto>>>
    {
        private readonly ILinkRepository _linkRepository;

        public GetAllLinkTypesHandler(ILinkRepository linkRepository)
        {
            _linkRepository = linkRepository;
        }

        public async Task<ApiResponse<List<LinkTypeDto>>> Handle(GetAllLinkTypesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var linkTypes = await _linkRepository.GetAllAsync();
                
                var linkTypeDtos = linkTypes.Select(lt => new LinkTypeDto
                {
                    Id = lt.Id,
                    Name = lt.Name ?? string.Empty
                }).ToList();

                return ApiResponse<List<LinkTypeDto>>.Success(linkTypeDtos, "Link types retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<LinkTypeDto>>.Fail($"Error retrieving link types: {ex.Message}");
            }
        }
    }
}