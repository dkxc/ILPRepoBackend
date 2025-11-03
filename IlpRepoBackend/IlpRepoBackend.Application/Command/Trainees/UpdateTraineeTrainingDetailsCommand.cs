using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrapper;
using MediatR;

namespace IlpRepoBackend.Application.Command.Trainees
{
    public class UpdateTraineeTrainingDetailsCommand : IRequest<ApiResponse<TraineeTrainingDetailsDto>>
    {
        public int TraineeId { get; set; }
        public string? BuddyName { get; set; }
        public string? DuName { get; set; }
        public string? OjtMentor { get; set; }
        public string? Location { get; set; }
    }
}
