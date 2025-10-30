using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrappers;
using MediatR;

namespace IlpRepoBackend.Application.Command
{
    public class UpdateProjectTechStackCommand : IRequest<ApiResponse<ProjectDetailsDto>>
    {
        public int ProjectId { get; set; }
        public List<string> TechStack { get; set; } = new();

        public UpdateProjectTechStackCommand(UpdateProjectTechStackDto dto)
        {
            ProjectId = dto.ProjectId;
            TechStack = dto.TechStack;
        }

        public UpdateProjectTechStackCommand() { }
    }
}