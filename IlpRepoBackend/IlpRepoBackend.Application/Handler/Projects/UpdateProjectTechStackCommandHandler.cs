using AutoMapper;
using IlpRepoBackend.Application.Command;
using IlpRepoBackend.Application.CustomException;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrappers;
using IlpRepoBackend.Domain.Persistence;
using MediatR;

namespace IlpRepoBackend.Application.Handler.Projects
{
    public class UpdateProjectTechStackCommandHandler : IRequestHandler<UpdateProjectTechStackCommand, ApiResponse<ProjectDetailsDto>>
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IMapper _mapper;

        public UpdateProjectTechStackCommandHandler(IProjectRepository projectRepository, IMapper mapper)
        {
            _projectRepository = projectRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<ProjectDetailsDto>> Handle(UpdateProjectTechStackCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Get the project
                var project = await _projectRepository.GetByIdAsync(request.ProjectId);

                if (project == null)
                {
                    throw new NotFoundException("Project", request.ProjectId);
                }

                // Update Technology (Tech Stack)
                var techStackString = string.Join(", ", request.TechStack.Where(tech => !string.IsNullOrWhiteSpace(tech)));
                project.Technology = string.IsNullOrWhiteSpace(techStackString) ? null : techStackString;
                project.UpdatedAt = DateTime.UtcNow;

                // Update the project
                await _projectRepository.UpdateProjectAsync(project);

                // Get updated project details for response
                var updatedProject = await _projectRepository.GetProjectDetailsAsync(request.ProjectId);
                var projectDetailsDto = _mapper.Map<ProjectDetailsDto>(updatedProject);

                return new ApiResponse<ProjectDetailsDto>(projectDetailsDto, "Project technology stack updated successfully");
            }
            catch (NotFoundException ex)
            {
                return new ApiResponse<ProjectDetailsDto>(ex.Message, 404);
            }
            catch (Exception ex)
            {
                return new ApiResponse<ProjectDetailsDto>($"An error occurred while updating project technology stack: {ex.Message}", 500);
            }
        }
    }
}