using IlpRepoBackend.Application.Dto.Documents;
using IlpRepoBackend.Application.Wrapper;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace IlpRepoBackend.Application.Command.Documents
{
    public class CreateDocumentTypeCommand : IRequest<ApiResponse<CreatedDocumentTypeDto>>
    {
        public string Name { get; set; }
        public IFormFile? TemplateFile { get; set; }

        public CreateDocumentTypeCommand(string name, IFormFile? templateFile = null)
        {
            Name = name;
            TemplateFile = templateFile;
        }
    }
}