using IlpRepoBackend.Application.Command.Projects;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handler.Projects
{
    public class UpsertProjectLinkHandler : IRequestHandler<UpsertProjectLinkCommand, ApiResponse<bool>>
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectLinkRepository _projectLinkRepository;
        private readonly ILinkRepository _linkRepository;

        public UpsertProjectLinkHandler(
            IProjectRepository projectRepository, 
            IProjectLinkRepository projectLinkRepository,
            ILinkRepository linkRepository)
        {
            _projectRepository = projectRepository;
            _projectLinkRepository = projectLinkRepository;
            _linkRepository = linkRepository;
        }

        public async Task<ApiResponse<bool>> Handle(UpsertProjectLinkCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Verify project exists
                var project = await _projectRepository.GetByIdAsync(request.ProjectId);
                if (project == null)
                {
                    return ApiResponse<bool>.Fail($"Project with ID {request.ProjectId} not found");
                }

                // Verify link type exists
                var linkType = await _linkRepository.GetByIdAsync(request.LinkId);
                if (linkType == null)
                {
                    return ApiResponse<bool>.Fail($"Link type with ID {request.LinkId} not found");
                }

                // Check if a ProjectLink already exists for this project and link type
                var existingProjectLinks = await _projectLinkRepository.GetByProjectIdAsync(request.ProjectId);
                var existingLink = existingProjectLinks.FirstOrDefault(pl => pl.LinkId == request.LinkId);

                bool isUpdate = false;

                if (existingLink != null)
                {
                    // UPDATE existing project link
                    existingLink.LinkUrl = request.LinkUrl;
                    existingLink.UpdatedAt = DateTime.UtcNow;
                    await _projectLinkRepository.UpdateAsync(existingLink);
                    isUpdate = true;
                }
                else
                {
                    // CREATE new project link
                    var newProjectLink = new ProjectLink
                    {
                        ProjectId = request.ProjectId,
                        LinkId = request.LinkId,
                        LinkUrl = request.LinkUrl,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _projectLinkRepository.AddAsync(newProjectLink);
                }

                // Update project's UpdatedAt
                project.UpdatedAt = DateTime.UtcNow;
                await _projectRepository.UpdateAsync(project);

                var action = isUpdate ? "updated" : "added";
                return ApiResponse<bool>.Success(true, $"Project link {action} successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Fail($"Error saving project link: {ex.Message}");
            }
        }
    }
}