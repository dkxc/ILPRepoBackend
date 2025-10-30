using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Query;
using IlpRepoBackend.Application.Wrappers;
using IlpRepoBackend.Domain.Persistence;
using MediatR;

namespace IlpRepoBackend.Application.Handler.Links
{
    public class GetAllLinkTypesQueryHandler : IRequestHandler<GetAllLinkTypesQuery, ApiResponse<List<LinkTypeDto>>>
    {
        private readonly ILinkRepository _linkRepository;

        public GetAllLinkTypesQueryHandler(ILinkRepository linkRepository)
        {
            _linkRepository = linkRepository;
        }

        public async Task<ApiResponse<List<LinkTypeDto>>> Handle(GetAllLinkTypesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var links = await _linkRepository.GetAllLinkTypesAsync();

                var linkTypeDtos = links.Select(l => new LinkTypeDto
                {
                    Id = l.Id,
                    Name = l.Name ?? string.Empty
                }).ToList();

                return new ApiResponse<List<LinkTypeDto>>(linkTypeDtos, "Link types retrieved successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<LinkTypeDto>>($"An error occurred: {ex.Message}", 500);
            }
        }
    }
}