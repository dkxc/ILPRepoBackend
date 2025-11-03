using IlpRepoBackend.Application.Command.Trainees;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handler.Trainees
{
    public class UpdateTraineeTrainingDetailsHandler : IRequestHandler<UpdateTraineeTrainingDetailsCommand, ApiResponse<TraineeTrainingDetailsDto>>
    {
        private readonly ITraineeRepository _traineeRepository;
        private readonly IBoPhaseRepository _boPhaseRepository;
        private readonly ITraineeDuRepository _traineeDuRepository;
        private readonly IBuddyRepository _buddyRepository;
        private readonly IDuRepository _duRepository;

        public UpdateTraineeTrainingDetailsHandler(
            ITraineeRepository traineeRepository,
            IBoPhaseRepository boPhaseRepository,
            ITraineeDuRepository traineeDuRepository,
            IBuddyRepository buddyRepository,
            IDuRepository duRepository)
        {
            _traineeRepository = traineeRepository;
            _boPhaseRepository = boPhaseRepository;
            _traineeDuRepository = traineeDuRepository;
            _buddyRepository = buddyRepository;
            _duRepository = duRepository;
        }

        public async Task<ApiResponse<TraineeTrainingDetailsDto>> Handle(
            UpdateTraineeTrainingDetailsCommand request,
            CancellationToken cancellationToken)
        {
            // Get trainee with user and batch details
            var trainee = await _traineeRepository.GetByIdAsync(request.TraineeId);
            if (trainee == null)
                return ApiResponse<TraineeTrainingDetailsDto>.Fail($"Trainee with ID {request.TraineeId} not found");

            // Update BoPhase (Buddy assignment) if provided
            if (!string.IsNullOrWhiteSpace(request.BuddyName))
            {
                var boPhases = await _boPhaseRepository.GetByTraineeIdAsync(request.TraineeId);
                var boPhase = boPhases.FirstOrDefault();

                // Get or create DU if provided
                int? duId = null;
                if (!string.IsNullOrWhiteSpace(request.DuName))
                {
                    var du = await _duRepository.GetByNameAsync(request.DuName);
                    if (du == null)
                    {
                        du = new Du
                        {
                            Name = request.DuName,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        du = await _duRepository.AddAsync(du);
                        if (du == null)
                            return ApiResponse<TraineeTrainingDetailsDto>.Fail($"Failed to create DU with name {request.DuName}");
                    }
                    duId = du.Id;
                }

                // Get or create Buddy
                var buddy = await _buddyRepository.GetByNameAsync(request.BuddyName);
                if (buddy == null)
                {
                    buddy = new Buddy
                    {
                        Name = request.BuddyName,
                        DuId = duId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    buddy = await _buddyRepository.AddAsync(buddy);
                    if (buddy == null)
                        return ApiResponse<TraineeTrainingDetailsDto>.Fail($"Failed to create Buddy with name {request.BuddyName}");
                }
                else if (duId.HasValue && buddy.DuId != duId)
                {
                    // Update buddy's DU if it has changed
                    buddy.DuId = duId;
                    buddy.UpdatedAt = DateTime.UtcNow;
                    await _buddyRepository.UpdateAsync(buddy);
                }

                // Update or create BoPhase
                if (boPhase == null)
                {
                    boPhase = new BoPhase
                    {
                        TraineeId = request.TraineeId,
                        BuddyId = buddy.Id,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _boPhaseRepository.AddAsync(boPhase);
                }
                else
                {
                    boPhase.BuddyId = buddy.Id;
                    boPhase.UpdatedAt = DateTime.UtcNow;
                    await _boPhaseRepository.UpdateAsync(boPhase);
                }
            }

            // Update TraineeDu (OJT Mentor, DU allocation, Location) if provided
            if (!string.IsNullOrWhiteSpace(request.OjtMentor) || 
                !string.IsNullOrWhiteSpace(request.DuName) || 
                !string.IsNullOrWhiteSpace(request.Location))
            {
                var traineeDus = await _traineeDuRepository.GetByTraineeIdAsync(request.TraineeId);
                var traineeDu = traineeDus.FirstOrDefault();

                // Get or create DU for TraineeDu
                Du? du = null;
                if (!string.IsNullOrWhiteSpace(request.DuName))
                {
                    du = await _duRepository.GetByNameAsync(request.DuName);
                    if (du == null)
                    {
                        du = new Du
                        {
                            Name = request.DuName,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        du = await _duRepository.AddAsync(du);
                        if (du == null)
                            return ApiResponse<TraineeTrainingDetailsDto>.Fail($"Failed to create DU with name {request.DuName}");
                    }
                }

                // Update or create TraineeDu
                if (traineeDu == null)
                {
                    traineeDu = new TraineeDu
                    {
                        TraineeId = request.TraineeId,
                        DuId = du?.Id,
                        Location = request.Location ?? string.Empty,
                        ojtMenter = request.OjtMentor ?? string.Empty,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _traineeDuRepository.AddAsync(traineeDu);
                }
                else
                {
                    if (du != null)
                        traineeDu.DuId = du.Id;
                    
                    if (!string.IsNullOrWhiteSpace(request.OjtMentor))
                        traineeDu.ojtMenter = request.OjtMentor;
                    
                    if (!string.IsNullOrWhiteSpace(request.Location))
                        traineeDu.Location = request.Location;
                    
                    traineeDu.UpdatedAt = DateTime.UtcNow;
                    await _traineeDuRepository.UpdateAsync(traineeDu);
                }
            }

            // Fetch updated details
            var updatedTrainee = await _traineeRepository.GetByIdAsync(request.TraineeId);
            var updatedBoPhases = await _boPhaseRepository.GetByTraineeIdAsync(request.TraineeId);
            var updatedBoPhase = updatedBoPhases.FirstOrDefault();
            var updatedTraineeDus = await _traineeDuRepository.GetByTraineeIdAsync(request.TraineeId);
            var updatedTraineeDu = updatedTraineeDus.FirstOrDefault();

            // Build the response
            var trainingDetails = new TraineeTrainingDetailsDto
            {
                TraineeId = updatedTrainee.Id,
                TraineeName = updatedTrainee.User?.Username ?? string.Empty,
                Email = updatedTrainee.Email ?? string.Empty,
                BatchName = updatedTrainee.Batch?.BatchName ?? string.Empty,
                BuddyName = updatedBoPhase?.Buddy?.Name,
                BuddyDU = updatedBoPhase?.Buddy?.Du?.Name,
                OjtMentor = updatedTraineeDu?.ojtMenter,
                DuAllocated = updatedTraineeDu?.Du?.Name,
                Location = updatedTraineeDu?.Location
            };

            return ApiResponse<TraineeTrainingDetailsDto>.Success(trainingDetails);
        }
    }
}
