using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrapper;
using MediatR;
using System.Collections.Generic;

namespace IlpRepoBackend.Application.Command.TraineeDus
{
    public class CreateTraineeDuByBatchCommand : IRequest<ApiResponse<List<TraineeDuDto>>>
    {
        public int BatchId { get; set; }
        public List<AddTraineeDuForBatchDto> TraineeDus { get; set; }
    }
}