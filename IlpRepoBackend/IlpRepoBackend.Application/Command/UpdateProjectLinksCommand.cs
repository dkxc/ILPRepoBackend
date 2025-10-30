using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrappers;
using MediatR;

namespace IlpRepoBackend.Application.Command
{
    public class UpdateProjectLinksCommand : IRequest<ApiResponse<ProjectDetailsDto>>
    {
        public int ProjectId { get; set; }
        public int TraineeId { get; set; } // WHO is making this change (must be Team Leader)
        public List<UpdateProjectLinkDto> ProjectLinks { get; set; } = new();

        public UpdateProjectLinksCommand(int projectId, int traineeId, List<UpdateProjectLinkDto> projectLinks)
        {
            ProjectId = projectId;
            TraineeId = traineeId;
            ProjectLinks = projectLinks;
        }
    }
}