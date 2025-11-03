using IlpRepoBackend.Application.Dto.DocumentSubmissions;
using IlpRepoBackend.Application.Wrapper;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace IlpRepoBackend.Application.Command.DocumentSubmissions
{
    public class SubmitDocumentCommand : IRequest<ApiResponse<DocumentSubmissionResultDto>>
    {
        public int DocumentRequestId { get; set; }
        public IFormFile DocumentFile { get; set; }

        public SubmitDocumentCommand(int documentRequestId, IFormFile documentFile)
        {
            DocumentRequestId = documentRequestId;
            DocumentFile = documentFile;
        }
    }
}