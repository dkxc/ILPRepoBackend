using AutoMapper;
using IlpRepoBackend.Application.Command.BoPhases;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handler.BoPhases
{
    public class CreateBoPhaseHandler : IRequestHandler<CreateBoPhaseCommand, ApiResponse<BoPhaseDetailsDto>>
    {
        private readonly IBoPhaseRepository _boPhaseRepository;
        private readonly ITraineeRepository _traineeRepository;
        private readonly IBuddyRepository _buddyRepository;
        private readonly IUserRepository _userRepository;
        private readonly IDuRepository _duRepository;
        private readonly IMapper _mapper;

        public CreateBoPhaseHandler(
            IBoPhaseRepository boPhaseRepository,
            ITraineeRepository traineeRepository,
            IBuddyRepository buddyRepository,
            IUserRepository userRepository,
            IDuRepository duRepository,
            IMapper mapper)
        {
            _boPhaseRepository = boPhaseRepository;
            _traineeRepository = traineeRepository;
            _buddyRepository = buddyRepository;
            _userRepository = userRepository;
            _duRepository = duRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<BoPhaseDetailsDto>> Handle(CreateBoPhaseCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.TraineeName))
                return ApiResponse<BoPhaseDetailsDto>.Fail("Trainee name cannot be empty");

            if (string.IsNullOrWhiteSpace(request.Email))
                return ApiResponse<BoPhaseDetailsDto>.Fail("Trainee email cannot be empty");

            if (string.IsNullOrWhiteSpace(request.BuddyName))
                return ApiResponse<BoPhaseDetailsDto>.Fail("Buddy name cannot be empty");

            // Validate trainee by email
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
                return ApiResponse<BoPhaseDetailsDto>.Fail($"Trainee with email {request.Email} not found");

            // Verify trainee name matches
            if (!user.Username.Equals(request.TraineeName, StringComparison.OrdinalIgnoreCase))
                return ApiResponse<BoPhaseDetailsDto>.Fail($"Trainee name {request.TraineeName} does not match email {request.Email}");

            var trainee = await _traineeRepository.GetByUserIdAsync(user.Id);
            if (trainee == null)
                return ApiResponse<BoPhaseDetailsDto>.Fail($"Trainee profile for {request.Email} not found");

            // Check if trainee already has a BO Phase assignment (by trainee ID, not name)
            var existingBoPhases = await _boPhaseRepository.GetByTraineeIdAsync(trainee.Id);
            if (existingBoPhases.Any())
                return ApiResponse<BoPhaseDetailsDto>.Fail($"Trainee with email {request.Email} already has a BO Phase assignment");

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
                        return ApiResponse<BoPhaseDetailsDto>.Fail($"Failed to create DU with name {request.DuName}");
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
                    return ApiResponse<BoPhaseDetailsDto>.Fail($"Failed to create Buddy with name {request.BuddyName}");
            }
            else if (duId.HasValue && buddy.DuId != duId)
            {
                // Update buddy's DU if it has changed
                buddy.DuId = duId;
                buddy.UpdatedAt = DateTime.UtcNow;
                await _buddyRepository.UpdateAsync(buddy);
            }

            // Create BOPhase
            var boPhase = new BoPhase
            {
                TraineeId = trainee.Id,
                BuddyId = buddy.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdBoPhase = await _boPhaseRepository.AddAsync(boPhase);
            if (createdBoPhase == null)
                return ApiResponse<BoPhaseDetailsDto>.Fail("Failed to create BO Phase record");

            // Get full record with includes
            var boPhaseWithIncludes = await _boPhaseRepository.GetByIdAsync(createdBoPhase.Id);
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