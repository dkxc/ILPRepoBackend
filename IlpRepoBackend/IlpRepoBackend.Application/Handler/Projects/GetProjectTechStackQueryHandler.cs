using IlpRepoBackend.Application.CustomException;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Query;
using IlpRepoBackend.Application.Wrappers;
using IlpRepoBackend.Domain.Persistence;
using MediatR;

namespace IlpRepoBackend.Application.Handler.Projects
{
    public class GetProjectTechStackQueryHandler : IRequestHandler<GetProjectTechStackQuery, ApiResponse<ProjectTechStackDto>>
    {
        private readonly IProjectRepository _projectRepository;

        public GetProjectTechStackQueryHandler(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task<ApiResponse<ProjectTechStackDto>> Handle(GetProjectTechStackQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var project = await _projectRepository.GetByIdAsync(request.ProjectId);

                if (project == null)
                {
                    throw new NotFoundException("Project", request.ProjectId);
                }

                var techStackDto = new ProjectTechStackDto
                {
                    Id = project.Id,
                    ProjectName = project.ProjectName,
                    Technology = project.Technology,
                    TechStack = !string.IsNullOrEmpty(project.Technology)
                        ? project.Technology.Split(new[] { ',' }, StringSplitOptions.None)
                            .Select(t => t.Trim())
                            .Where(t => !string.IsNullOrWhiteSpace(t))
                            .ToList()
                        : new List<string>()
                };

                return new ApiResponse<ProjectTechStackDto>(techStackDto, "Project tech stack retrieved successfully");
            }
            catch (NotFoundException ex)
            {
                return new ApiResponse<ProjectTechStackDto>(ex.Message, 404);
            }
            catch (Exception ex)
            {
                return new ApiResponse<ProjectTechStackDto>($"An error occurred: {ex.Message}", 500);
            }
        }
    }
}