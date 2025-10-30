using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrappers;
using MediatR;

namespace IlpRepoBackend.Application.Command
{
    public class UploadDocumentSubmissionCommand : IRequest<ApiResponse<DocumentSubmissionDto>>
    {
        public int RequestId { get; set; }
        public int DocumentId { get; set; }
        public int TraineeId { get; set; }
        public string SubmissionLink { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;

        public UploadDocumentSubmissionCommand() { }

        public UploadDocumentSubmissionCommand(int requestId, int documentId, int traineeId, string submissionLink, string fileName, string fileType)
        {
            RequestId = requestId;
            DocumentId = documentId;
            TraineeId = traineeId;
            SubmissionLink = submissionLink;
            FileName = fileName;
            FileType = fileType;
        }
    }
}