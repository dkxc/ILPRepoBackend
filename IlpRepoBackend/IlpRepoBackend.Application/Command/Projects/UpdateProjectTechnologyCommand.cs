using IlpRepoBackend.Application.Wrapper;
using MediatR;

namespace IlpRepoBackend.Application.Command.Projects
{
    public class UpdateProjectTechnologyCommand : IRequest<ApiResponse<bool>>
    {
        public int ProjectId { get; set; }
        public string Technology { get; set; } = string.Empty;

        public UpdateProjectTechnologyCommand(int projectId, string technology)
        {
            ProjectId = projectId;
            Technology = technology;
        }
    }
}