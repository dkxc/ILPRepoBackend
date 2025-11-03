using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrapper;
using MediatR;

namespace IlpRepoBackend.Application.Command.BatchTypes
{
    public class UpdateBatchTypeCommand : IRequest<ApiResponse<BatchTypeDto>>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}