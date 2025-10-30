using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrappers;
using MediatR;

namespace IlpRepoBackend.Application.Command
{
    public class UpdateDocumentRequestCommand : IRequest<ApiResponse<DocumentRequestResponseDto>>
    {
        public int Id { get; set; }
        public int BatchId { get; set; }
        public int DocumentId { get; set; }
        public DateTime DueDate { get; set; }
        public string? FileUrl { get; set; }
        public string? OldFileUrl { get; set; }
        public bool RemoveExistingFile { get; set; }

        public UpdateDocumentRequestCommand() { }

        public UpdateDocumentRequestCommand(int id, int batchId, int documentId, DateTime dueDate, 
            string? fileUrl = null, string? oldFileUrl = null, bool removeExistingFile = false)
        {
            Id = id;
            BatchId = batchId;
            DocumentId = documentId;
            DueDate = dueDate;
            FileUrl = fileUrl;
            OldFileUrl = oldFileUrl;
            RemoveExistingFile = removeExistingFile;
        }
    }
}