using IlpRepoBackend.Application.Dto.Results;
using IlpRepoBackend.Application.Wrapper;
using MediatR;

namespace IlpRepoBackend.Application.Queries.Results
{
    public class GetTraineeResultsQuery : IRequest<ApiResponse<TraineeDetailedResultsDto>>
    {
        public int TraineeId { get; set; }
    }
}