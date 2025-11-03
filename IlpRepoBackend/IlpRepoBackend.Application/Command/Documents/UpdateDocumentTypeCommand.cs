using IlpRepoBackend.Application.Dto.Documents;
using IlpRepoBackend.Application.Wrapper;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace IlpRepoBackend.Application.Command.Documents
{
    public class UpdateDocumentTypeCommand : IRequest<ApiResponse<DocumentTypeDto>>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IFormFile? TemplateFile { get; set; }

        public UpdateDocumentTypeCommand(int id, string name, IFormFile? templateFile = null)
        {
            Id = id;
            Name = name;
            TemplateFile = templateFile;
        }
    }
}