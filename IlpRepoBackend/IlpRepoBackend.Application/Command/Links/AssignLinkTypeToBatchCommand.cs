using IlpRepoBackend.Application.Wrapper;
using MediatR;

namespace IlpRepoBackend.Application.Command.Links
{
    public class AssignLinkTypeToBatchCommand : IRequest<ApiResponse<bool>>
    {
        public int BatchId { get; set; }
        public int LinkTypeId { get; set; }

        public AssignLinkTypeToBatchCommand(int batchId, int linkTypeId)
        {
            BatchId = batchId;
            LinkTypeId = linkTypeId;
        }
    }
}