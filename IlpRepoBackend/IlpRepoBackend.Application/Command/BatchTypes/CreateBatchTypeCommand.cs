using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrapper;
using MediatR;

namespace IlpRepoBackend.Application.Command.BatchTypes
{
    public class CreateBatchTypeCommand : IRequest<ApiResponse<BatchTypeDto>>
    {
        public string Name { get; set; } = string.Empty;
    }
}