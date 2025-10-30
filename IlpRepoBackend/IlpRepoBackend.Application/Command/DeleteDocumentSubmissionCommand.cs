using IlpRepoBackend.Application.Wrappers;
using MediatR;

namespace IlpRepoBackend.Application.Command
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