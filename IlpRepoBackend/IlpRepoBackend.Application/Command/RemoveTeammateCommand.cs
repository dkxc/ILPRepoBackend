using IlpRepoBackend.Application.Wrappers;
using MediatR;

namespace IlpRepoBackend.Application.Command
{
    public class RemoveTeammateCommand : IRequest<ApiResponse<bool>>
    {
        public int ProjectId { get; set; }
        public int TraineeId { get; set; }

        public RemoveTeammateCommand(int projectId, int traineeId)
        {
            ProjectId = projectId;
            TraineeId = traineeId;
        }

        public RemoveTeammateCommand() { }
    }
}