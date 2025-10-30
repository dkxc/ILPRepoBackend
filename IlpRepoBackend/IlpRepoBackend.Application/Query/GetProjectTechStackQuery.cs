using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrappers;
using MediatR;

namespace IlpRepoBackend.Application.Query
{
    public class GetProjectTechStackQuery : IRequest<ApiResponse<ProjectTechStackDto>>
    {
        public int ProjectId { get; set; }

        public GetProjectTechStackQuery(int projectId)
        {
            ProjectId = projectId;
        }
    }
}