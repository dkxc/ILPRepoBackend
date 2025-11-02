using IlpRepoBackend.Application.Command.Links;
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
    public class AssignLinkTypeToBatchHandler : IRequestHandler<AssignLinkTypeToBatchCommand, ApiResponse<bool>>
    {
        private readonly IBatchRepository _batchRepository;
        private readonly ILinkRepository _linkRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectLinkRepository _projectLinkRepository;

        public AssignLinkTypeToBatchHandler(
            IBatchRepository batchRepository,
            ILinkRepository linkRepository,
            IProjectRepository projectRepository,
            IProjectLinkRepository projectLinkRepository)
        {
            _batchRepository = batchRepository;
            _linkRepository = linkRepository;
            _projectRepository = projectRepository;
            _projectLinkRepository = projectLinkRepository;
        }

        public async Task<ApiResponse<bool>> Handle(AssignLinkTypeToBatchCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // 1. Verify batch exists
                var batch = await _batchRepository.GetByIdAsync(request.BatchId);
                if (batch == null)
                {
                    return ApiResponse<bool>.Fail($"Batch with ID {request.BatchId} not found");
                }

                // 2. Verify link type exists
                var linkType = await _linkRepository.GetByIdAsync(request.LinkTypeId);
                if (linkType == null)
                {
                    return ApiResponse<bool>.Fail($"Link type with ID {request.LinkTypeId} not found");
                }

                // 3. Get all projects in this batch
                var projectsInBatch = await _projectRepository.GetProjectsByBatchIdAsync(request.BatchId);
                
                if (!projectsInBatch.Any())
                {
                    return ApiResponse<bool>.Fail($"No projects found for batch {request.BatchId}");
                }

                int createdLinks = 0;
                int existingLinks = 0;

                // 4. Create ProjectLink records for each project that doesn't already have this link type
                foreach (var project in projectsInBatch)
                {
                    // Check if this project already has this link type
                    var existingProjectLinks = await _projectLinkRepository.GetByProjectIdAsync(project.Id);
                    var hasLinkType = existingProjectLinks.Any(pl => pl.LinkId == request.LinkTypeId);

                    if (!hasLinkType)
                    {
                        // Create new ProjectLink with empty URL (required but not yet provided)
                        var newProjectLink = new ProjectLink
                        {
                            ProjectId = project.Id,
                            LinkId = request.LinkTypeId,
                            LinkUrl = null, // URL to be provided later by user
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        await _projectLinkRepository.AddAsync(newProjectLink);
                        createdLinks++;
                    }
                    else
                    {
                        existingLinks++;
                    }
                }

                var message = $"Link type '{linkType.Name}' assigned to batch '{batch.BatchName}'. " +
                             $"Created {createdLinks} new requirements, {existingLinks} already existed.";

                return ApiResponse<bool>.Success(true, message);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Fail($"Error assigning link type to batch: {ex.Message}");
            }
        }
    }
}