using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrappers;
using MediatR;

namespace IlpRepoBackend.Application.Query
{
    public class GetDocumentSubmissionQuery : IRequest<ApiResponse<DocumentSubmissionDto>>
    {
        public int SubmissionId { get; set; }

        public GetDocumentSubmissionQuery(int submissionId)
        {
            SubmissionId = submissionId;
        }
    }
}