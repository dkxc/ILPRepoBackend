using IlpRepoBackend.Application.Dto.Links;
using IlpRepoBackend.Application.Wrapper;
using MediatR;

namespace IlpRepoBackend.Application.Command.Links
{
    public class CreateLinkTypeCommand : IRequest<ApiResponse<CreatedLinkTypeDto>>
    {
        public string Name { get; set; }

        public CreateLinkTypeCommand(string name)
        {
            Name = name;
        }
    }
}