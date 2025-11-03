using AutoMapper;
using IlpRepoBackend.Application.Command.BoPhases;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace IlpRepoBackend.Application.Handler.BoPhases
{
    public class UpdateBoPhaseHandler : IRequestHandler<UpdateBoPhaseCommand, ApiResponse<BoPhaseDetailsDto>>
    {
        private readonly IBoPhaseRepository _boPhaseRepository;
        private readonly IBuddyRepository _buddyRepository;
        private readonly ITraineeRepository _traineeRepository;
        private readonly IUserRepository _userRepository;
        private readonly IDuRepository _duRepository;
        private readonly IMapper _mapper;

        public UpdateBoPhaseHandler(
            IBoPhaseRepository boPhaseRepository,
            IBuddyRepository buddyRepository,
            ITraineeRepository traineeRepository,
            IUserRepository userRepository,
            IDuRepository duRepository,
            IMapper mapper)
        {
            _boPhaseRepository = boPhaseRepository;
            _buddyRepository = buddyRepository;
            _traineeRepository = traineeRepository;
            _userRepository = userRepository;
            _duRepository = duRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<BoPhaseDetailsDto>> Handle(UpdateBoPhaseCommand request, CancellationToken cancellationToken)
        {
            // Get existing BoPhase
            var existingBoPhase = await _boPhaseRepository.GetByIdAsync(request.BoPhaseId);
            if (existingBoPhase == null)
                return ApiResponse<BoPhaseDetailsDto>.Fail($"BO Phase with ID {request.BoPhaseId} not found");

            // If trainee name is provided, update trainee
            if (!string.IsNullOrWhiteSpace(request.TraineeName))
            {
                var user = await _userRepository.GetByUsernameAsync(request.TraineeName);
                if (user == null)
                    return ApiResponse<BoPhaseDetailsDto>.Fail($"Trainee with name {request.TraineeName} not found");

                var trainee = await _traineeRepository.GetByUserIdAsync(user.Id);
                if (trainee == null)
                    return ApiResponse<BoPhaseDetailsDto>.Fail($"Trainee profile for {request.TraineeName} not found");

                // Only check for duplicate assignments if we're actually changing the trainee
                if (existingBoPhase.TraineeId != trainee.Id)
                {
                    // Check if this trainee already has a BO Phase assignment
                    var existingTraineeBoPhases = await _boPhaseRepository.GetByTraineeIdAsync(trainee.Id);
                    if (existingTraineeBoPhases.Any())
                        return ApiResponse<BoPhaseDetailsDto>.Fail($"Trainee {request.TraineeName} already has a BO Phase assignment");
                }

                existingBoPhase.TraineeId = trainee.Id;
            }

            // If DU name is provided, get or create DU
            int? duId = null;
            if (!string.IsNullOrWhiteSpace(request.DuName))
            {
                var du = await _duRepository.GetByNameAsync(request.DuName);
                if (du == null)
                {
                    // Create new DU
                    du = new Du
                    {
                        Name = request.DuName,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    du = await _duRepository.AddAsync(du);
                    if (du == null)
                        return ApiResponse<BoPhaseDetailsDto>.Fail($"Failed to create DU with name {request.DuName}");
                }
                duId = du.Id;
            }

            // Update or create buddy
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
                    return ApiResponse<BoPhaseDetailsDto>.Fail($"Failed to create Buddy with name {request.BuddyName}");
            }
            else if (duId.HasValue && buddy.DuId != duId)
            {
                // Update buddy's DU if it has changed
                buddy.DuId = duId;
                buddy.UpdatedAt = DateTime.UtcNow;
                await _buddyRepository.UpdateAsync(buddy);
            }

            // Update BoPhase
            existingBoPhase.BuddyId = buddy.Id;
            existingBoPhase.UpdatedAt = DateTime.UtcNow;

            var updatedBoPhase = await _boPhaseRepository.UpdateAsync(existingBoPhase);
            
            // Get full record with includes
            var boPhaseWithIncludes = await _boPhaseRepository.GetByIdAsync(updatedBoPhase.Id);
            var boPhaseDto = new BoPhaseDetailsDto
            {
                BoPhaseId = boPhaseWithIncludes.Id,
                TraineeName = boPhaseWithIncludes.Trainee?.User?.Username,
                Buddy = boPhaseWithIncludes.Buddy?.Name,
                BuddyDU = boPhaseWithIncludes.Buddy?.Du?.Name
            };

            return ApiResponse<BoPhaseDetailsDto>.Success(boPhaseDto);
        }
    }
}