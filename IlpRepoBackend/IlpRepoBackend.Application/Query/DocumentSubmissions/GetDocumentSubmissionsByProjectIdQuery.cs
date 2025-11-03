using IlpRepoBackend.Application.Dto.DocumentSubmissions;
using IlpRepoBackend.Application.Wrapper;
using MediatR;

namespace IlpRepoBackend.Application.Query.DocumentSubmissions
{
    public class GetDocumentSubmissionsByProjectIdQuery : IRequest<ApiResponse<List<ProjectDocumentSubmissionInfo>>>
    {
        public int ProjectId { get; set; }

        public GetDocumentSubmissionsByProjectIdQuery(int projectId)
        {
            ProjectId = projectId;
        }
    }
}