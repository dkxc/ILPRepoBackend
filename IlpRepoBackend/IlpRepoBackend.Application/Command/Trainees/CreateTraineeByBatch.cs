using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrapper;
using MediatR;
using System.Collections.Generic;

namespace IlpRepoBackend.Application.Command.Trainees
{
    public class CreateTraineeByBatch : IRequest<ApiResponse<List<TraineeDto>>>
    {
        public int BatchId { get; set; }
        public List<AddTraineeForABatchDto> Trainees { get; set; }

    }
}
