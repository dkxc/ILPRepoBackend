using IlpRepoBackend.Application.Dto.Results;
using IlpRepoBackend.Application.Queries.Results;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handlers.Results
{
    public class GetScoresByBatchQueryHandler : IRequestHandler<GetScoresByBatchQuery, ApiResponse<List<TraineeScoreDto>>>
    {
        private readonly ITraineeRepository _traineeRepository;
        private readonly IResultRepository _resultRepository;

        public GetScoresByBatchQueryHandler(ITraineeRepository traineeRepository, IResultRepository resultRepository)
        {
            _traineeRepository = traineeRepository;
            _resultRepository = resultRepository;
        }

        public async Task<ApiResponse<List<TraineeScoreDto>>> Handle(GetScoresByBatchQuery request, CancellationToken cancellationToken)
        {
            var trainees = await _traineeRepository.GetByBatchIdAsync(request.BatchId);
            if (!trainees.Any())
                return ApiResponse<List<TraineeScoreDto>>.Fail("No trainees found for this batch.");

            var traineeScoreList = new List<TraineeScoreDto>();
            foreach (var trainee in trainees)
            {
                var results = await _resultRepository.GetByTraineeIdAsync(trainee.Id);
                var traineeScoreDto = new TraineeScoreDto
                {
                    TraineeId = trainee.Id,
                    Name = trainee.User.Username,
                    Email = trainee.User.Email,
                    Scores = results.ToDictionary(r => r.Assessment.Type.ToString(), r => (int?)r.ObtainedMark)
                };
                traineeScoreList.Add(traineeScoreDto);
            }

            return ApiResponse<List<TraineeScoreDto>>.Success(traineeScoreList);
        }
    }
}