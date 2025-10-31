using IlpRepoBackend.Application.Command.Links;
using IlpRepoBackend.Application.Dto.Links;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handler.Links
{
    public class CreateLinkTypeHandler : IRequestHandler<CreateLinkTypeCommand, ApiResponse<CreatedLinkTypeDto>>
    {
        private readonly ILinkRepository _linkRepository;

        public CreateLinkTypeHandler(ILinkRepository linkRepository)
        {
            _linkRepository = linkRepository;
        }

        public async Task<ApiResponse<CreatedLinkTypeDto>> Handle(CreateLinkTypeCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    return ApiResponse<CreatedLinkTypeDto>.Fail("Link type name cannot be empty");
                }

                // Check if link type with this name already exists
                var existingLinks = await _linkRepository.GetAllAsync();
                var duplicateLink = existingLinks.FirstOrDefault(l => 
                    string.Equals(l.Name?.Trim(), request.Name.Trim(), StringComparison.OrdinalIgnoreCase));

                if (duplicateLink != null)
                {
                    return ApiResponse<CreatedLinkTypeDto>.Fail($"Link type '{request.Name}' already exists");
                }

                // Create new link type
                var newLinkType = new Link
                {
                    Name = request.Name.Trim(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdLinkType = await _linkRepository.AddAsync(newLinkType);

                // Map to clean DTO
                var responseDto = new CreatedLinkTypeDto
                {
                    Id = createdLinkType.Id,
                    Name = createdLinkType.Name ?? string.Empty,
                    CreatedAt = createdLinkType.CreatedAt,
                    UpdatedAt = createdLinkType.UpdatedAt
                };

                return ApiResponse<CreatedLinkTypeDto>.Success(responseDto, "Link type created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<CreatedLinkTypeDto>.Fail($"Error creating link type: {ex.Message}");
            }
        }
    }
}