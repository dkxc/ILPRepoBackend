using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrapper;
using MediatR;

namespace IlpRepoBackend.Application.Query.Trainees
{
    public class GetTraineeTrainingDetailsQuery : IRequest<ApiResponse<TraineeTrainingDetailsDto>>
    {
        public int TraineeId { get; set; }

        public GetTraineeTrainingDetailsQuery(int traineeId)
        {
            TraineeId = traineeId;
        }
    }
}
