using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrappers;
using MediatR;

namespace IlpRepoBackend.Application.Query
{
    public class GetProjectDetailsQuery : IRequest<ApiResponse<ProjectDetailsDto>>
    {
        public int ProjectId { get; set; }

        public GetProjectDetailsQuery(int projectId)
        {
            ProjectId = projectId;
        }
    }
}
