using IlpRepoBackend.Application.Dto.Project;
using IlpRepoBackend.Application.Wrapper;
using MediatR;

namespace IlpRepoBackend.Application.Query.Projects
{
    public class GetProjectByIdQuery : IRequest<ApiResponse<ProjectDto>>
    {
        public int ProjectId { get; set; }

        public GetProjectByIdQuery(int projectId)
        {
            ProjectId = projectId;
        }
    }
}