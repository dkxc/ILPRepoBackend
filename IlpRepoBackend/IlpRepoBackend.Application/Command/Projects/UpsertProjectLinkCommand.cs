using IlpRepoBackend.Application.Wrapper;
using MediatR;

namespace IlpRepoBackend.Application.Command.Projects
{
    public class UpsertProjectLinkCommand : IRequest<ApiResponse<bool>>
    {
        public int ProjectId { get; set; }
        public int LinkId { get; set; } // Link type ID
        public string LinkUrl { get; set; } = string.Empty;

        public UpsertProjectLinkCommand(int projectId, int linkId, string linkUrl)
        {
            ProjectId = projectId;
            LinkId = linkId;
            LinkUrl = linkUrl;
        }
    }
}