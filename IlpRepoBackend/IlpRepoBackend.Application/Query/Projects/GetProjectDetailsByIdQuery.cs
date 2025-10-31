using IlpRepoBackend.Application.Dto.Project;
using IlpRepoBackend.Application.Wrapper;
using MediatR;

namespace IlpRepoBackend.Application.Query.Projects
{
    public class GetProjectDetailsByIdQuery : IRequest<ApiResponse<ProjectDetailsDto>>
    {
        public int ProjectId { get; set; }

        public GetProjectDetailsByIdQuery(int projectId)
        {
            ProjectId = projectId;
        }
    }
}
