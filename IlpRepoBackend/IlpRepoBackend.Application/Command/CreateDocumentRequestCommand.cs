using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrappers;
using MediatR;

namespace IlpRepoBackend.Application.Command
{
    public class CreateDocumentRequestCommand : IRequest<ApiResponse<DocumentRequestResponseDto>>
    {
        public int BatchId { get; set; }
        public int DocumentId { get; set; }
        public DateTime DueDate { get; set; }
        public string? FileUrl { get; set; }

        public CreateDocumentRequestCommand() { }

        public CreateDocumentRequestCommand(int batchId, int documentId, DateTime dueDate, string? fileUrl = null)
        {
            BatchId = batchId;
            DocumentId = documentId;
            DueDate = dueDate;
            FileUrl = fileUrl;
        }
    }
}