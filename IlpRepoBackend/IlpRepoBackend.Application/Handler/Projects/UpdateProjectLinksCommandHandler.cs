using AutoMapper;
using IlpRepoBackend.Application.Command;
using IlpRepoBackend.Application.CustomException;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Services;
using IlpRepoBackend.Application.Wrappers;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using MediatR;

namespace IlpRepoBackend.Application.Handler.Projects
{
    public class UpdateProjectLinksCommandHandler : IRequestHandler<UpdateProjectLinksCommand, ApiResponse<ProjectDetailsDto>>
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectAuthorizationService _authorizationService;
        private readonly IMapper _mapper;

        public UpdateProjectLinksCommandHandler(
            IProjectRepository projectRepository,
            IProjectAuthorizationService authorizationService,
            IMapper mapper)
        {
            _projectRepository = projectRepository;
            _authorizationService = authorizationService;
            _mapper = mapper;
        }

        public async Task<ApiResponse<ProjectDetailsDto>> Handle(UpdateProjectLinksCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // AUTHORIZATION: Check if trainee is Team Leader of this project
                var isTeamLeader = await _authorizationService.IsTeamLeaderAsync(request.TraineeId, request.ProjectId);
                if (!isTeamLeader)
                {
                    return new ApiResponse<ProjectDetailsDto>(
                        "Unauthorized: Only the Team Leader can update project links", 
                        403);
                }

                // Get the project with its links
                var project = await _projectRepository.GetProjectWithLinksAsync(request.ProjectId);

                if (project == null)
                {
                    throw new NotFoundException("Project", request.ProjectId);
                }

                // Update project timestamp
                project.UpdatedAt = DateTime.UtcNow;

                // Process Project Links
                await UpdateProjectLinks(project, request.ProjectLinks);

                // Update the project
                await _projectRepository.UpdateProjectAsync(project);

                // Get updated project details for response
                var updatedProject = await _projectRepository.GetProjectDetailsAsync(request.ProjectId);
                var projectDetailsDto = _mapper.Map<ProjectDetailsDto>(updatedProject);

                return new ApiResponse<ProjectDetailsDto>(projectDetailsDto, "Project links updated successfully");
            }
            catch (NotFoundException ex)
            {
                return new ApiResponse<ProjectDetailsDto>(ex.Message, 404);
            }
            catch (Exception ex)
            {
                return new ApiResponse<ProjectDetailsDto>($"An error occurred while updating project links: {ex.Message}", 500);
            }
        }

        private async Task UpdateProjectLinks(Project project, List<UpdateProjectLinkDto> requestLinks)
        {
            // Get existing project links
            var existingLinks = project.ProjectLinks.ToList();

            // Handle deletions - remove links marked for deletion
            var linksToDelete = requestLinks
                .Where(rl => rl.IsDeleted && rl.Id.HasValue)
                .Join(existingLinks, rl => rl.Id.Value, el => el.Id, (rl, el) => el)
                .ToList();

            if (linksToDelete.Any())
            {
                await _projectRepository.RemoveProjectLinksAsync(linksToDelete);
                // Remove from project collection
                foreach (var link in linksToDelete)
                {
                    project.ProjectLinks.Remove(link);
                }
            }

            // Handle updates - update existing links that are not marked for deletion
            var linksToUpdate = requestLinks
                .Where(rl => !rl.IsDeleted && rl.Id.HasValue)
                .ToList();

            foreach (var requestLink in linksToUpdate)
            {
                var existingLink = existingLinks.FirstOrDefault(el => el.Id == requestLink.Id.Value);
                if (existingLink != null)
                {
                    existingLink.LinkUrl = requestLink.LinkUrl;
                    existingLink.LinkId = requestLink.LinkId;
                    existingLink.UpdatedAt = DateTime.UtcNow;
                }
            }

            // Handle additions - create new links
            var newLinks = requestLinks
                .Where(rl => !rl.IsDeleted && !rl.Id.HasValue)
                .Select(rl => new ProjectLink
                {
                    ProjectId = project.Id,
                    LinkId = rl.LinkId,
                    LinkUrl = rl.LinkUrl,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                })
                .ToList();

            if (newLinks.Any())
            {
                await _projectRepository.AddProjectLinksAsync(newLinks);
                // Add to project collection
                foreach (var link in newLinks)
                {
                    project.ProjectLinks.Add(link);
                }
            }
        }
    }
}