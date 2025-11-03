using IlpRepoBackend.Application.Wrapper;
using MediatR;

namespace IlpRepoBackend.Application.Command.BatchTypes
{
    public class DeleteBatchTypeCommand : IRequest<ApiResponse<bool>>
    {
        public int Id { get; set; }
    }
}
