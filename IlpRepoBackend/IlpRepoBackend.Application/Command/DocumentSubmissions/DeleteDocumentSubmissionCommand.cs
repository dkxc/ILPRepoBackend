using IlpRepoBackend.Application.Wrapper;
using MediatR;

namespace IlpRepoBackend.Application.Command.DocumentSubmissions
{
    public class DeleteDocumentSubmissionCommand : IRequest<ApiResponse<bool>>
    {
        public int SubmissionId { get; set; }

        public DeleteDocumentSubmissionCommand(int submissionId)
        {
            SubmissionId = submissionId;
        }
    }
}