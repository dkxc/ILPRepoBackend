using IlpRepoBackend.Application.Dto.Results;
using IlpRepoBackend.Application.Queries.Results;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handlers.Results
{
    public class GetTraineeResultsQueryHandler : IRequestHandler<GetTraineeResultsQuery, ApiResponse<TraineeDetailedResultsDto>>
    {
        private readonly ITraineeRepository _traineeRepository;
        private readonly IResultRepository _resultRepository;
        private readonly IFeedbackRepository _feedbackRepository;

        public GetTraineeResultsQueryHandler(ITraineeRepository traineeRepository, IResultRepository resultRepository, IFeedbackRepository feedbackRepository)
        {
            _traineeRepository = traineeRepository;
            _resultRepository = resultRepository;
            _feedbackRepository = feedbackRepository;
        }

        public async Task<ApiResponse<TraineeDetailedResultsDto>> Handle(GetTraineeResultsQuery request, CancellationToken cancellationToken)
        {
            var trainee = await _traineeRepository.GetByIdAsync(request.TraineeId);
            if (trainee == null)
                return ApiResponse<TraineeDetailedResultsDto>.Fail("Trainee not found.");

            var results = await _resultRepository.GetByTraineeIdAsync(trainee.Id);
            
            // This line will now work correctly because the interface returns IEnumerable<Feedback>
            var feedbacks = await _feedbackRepository.GetByTraineeIdAsync(trainee.Id);

            var detailedResults = new TraineeDetailedResultsDto
            {
                TraineeId = trainee.Id,
                Name = trainee.User.Username,
                Email = trainee.User.Email
            };

            foreach(var result in results)
            {
                var phaseResult = new PhaseResultDto
                {
                    PhaseName = result.Assessment.Type.ToString(),
                    Score = result.ObtainedMark,
                };
                
                // This line is now valid because 'feedbacks' is a list.
                var feedbackForPhase = feedbacks?.FirstOrDefault(f => f.AssessmentType == result.Assessment.Type);
                if (feedbackForPhase != null)
                {
                    phaseResult.Feedback = feedbackForPhase.FeedbackHeaderResponses.ToDictionary(fhr => fhr.FeedbackHeader.Header, fhr => fhr.Text);
                }

                detailedResults.Phases.Add(phaseResult);
            }

            return ApiResponse<TraineeDetailedResultsDto>.Success(detailedResults);
        }
    }
}