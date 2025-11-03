using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Query.Trainees;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handler.Trainees
{
    public class GetTraineeTrainingDetailsHandler : IRequestHandler<GetTraineeTrainingDetailsQuery, ApiResponse<TraineeTrainingDetailsDto>>
    {
        private readonly ITraineeRepository _traineeRepository;
        private readonly IBoPhaseRepository _boPhaseRepository;
        private readonly ITraineeDuRepository _traineeDuRepository;

        public GetTraineeTrainingDetailsHandler(
            ITraineeRepository traineeRepository,
            IBoPhaseRepository boPhaseRepository,
            ITraineeDuRepository traineeDuRepository)
        {
            _traineeRepository = traineeRepository;
            _boPhaseRepository = boPhaseRepository;
            _traineeDuRepository = traineeDuRepository;
        }

        public async Task<ApiResponse<TraineeTrainingDetailsDto>> Handle(
            GetTraineeTrainingDetailsQuery request,
            CancellationToken cancellationToken)
        {
            // Get trainee with user and batch details
            var trainee = await _traineeRepository.GetByIdAsync(request.TraineeId);
            if (trainee == null)
                return ApiResponse<TraineeTrainingDetailsDto>.Fail($"Trainee with ID {request.TraineeId} not found");

            // Get BO Phase details (Buddy information)
            var boPhases = await _boPhaseRepository.GetByTraineeIdAsync(request.TraineeId);
            var boPhase = boPhases.FirstOrDefault();

            // Get TraineeDu details (OJT Mentor, DU allocation, Location)
            var traineeDus = await _traineeDuRepository.GetByTraineeIdAsync(request.TraineeId);
            var traineeDu = traineeDus.FirstOrDefault();

            // Build the training details DTO
            var trainingDetails = new TraineeTrainingDetailsDto
            {
                TraineeId = trainee.Id,
                TraineeName = trainee.User?.Username ?? string.Empty,
                Email = trainee.Email ?? string.Empty,
                BatchName = trainee.Batch?.BatchName ?? string.Empty,
                BuddyName = boPhase?.Buddy?.Name,
                BuddyDU = boPhase?.Buddy?.Du?.Name,
                OjtMentor = traineeDu?.ojtMenter,
                DuAllocated = traineeDu?.Du?.Name,
                Location = traineeDu?.Location
            };

            return ApiResponse<TraineeTrainingDetailsDto>.Success(trainingDetails);
        }
    }
}
