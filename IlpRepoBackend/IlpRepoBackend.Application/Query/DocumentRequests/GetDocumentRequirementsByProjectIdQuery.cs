using IlpRepoBackend.Application.Dto.DocumentRequests;
using IlpRepoBackend.Application.Wrapper;
using MediatR;

namespace IlpRepoBackend.Application.Query.DocumentRequests
{
    public class GetDocumentRequirementsByProjectIdQuery : IRequest<ApiResponse<List<ProjectDocumentRequirementDto>>>
    {
        public int ProjectId { get; set; }

        public GetDocumentRequirementsByProjectIdQuery(int projectId)
        {
            ProjectId = projectId;
        }
    }
}