using AutoMapper;
using IlpRepoBackend.Application.Command.BoPhases;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handler.BoPhases
{
    public class CreateBoPhaseByBatchHandler : IRequestHandler<CreateBoPhaseByBatchCommand, ApiResponse<List<BoPhaseDetailsDto>>>
    {
        private readonly IBoPhaseRepository _boPhaseRepository;
        private readonly ITraineeRepository _traineeRepository;
        private readonly IBuddyRepository _buddyRepository;
        private readonly IUserRepository _userRepository;
        private readonly IBatchRepository _batchRepository;
        private readonly IDuRepository _duRepository;
        private readonly IMapper _mapper;

        public CreateBoPhaseByBatchHandler(
            IBoPhaseRepository boPhaseRepository,
            ITraineeRepository traineeRepository,
            IBuddyRepository buddyRepository,
            IUserRepository userRepository,
            IBatchRepository batchRepository,
            IDuRepository duRepository,
            IMapper mapper)
        {
            _boPhaseRepository = boPhaseRepository;
            _traineeRepository = traineeRepository;
            _buddyRepository = buddyRepository;
            _userRepository = userRepository;
            _batchRepository = batchRepository;
            _duRepository = duRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<List<BoPhaseDetailsDto>>> Handle(CreateBoPhaseByBatchCommand request, CancellationToken cancellationToken)
        {
            if (request?.BoPhases == null || !request.BoPhases.Any())
                return ApiResponse<List<BoPhaseDetailsDto>>.Fail("No BO Phase records to add");

            // Validate batch exists
            var batch = await _batchRepository.GetByIdAsync(request.BatchId);
            if (batch == null)
                return ApiResponse<List<BoPhaseDetailsDto>>.Fail("Batch does not exist");

            var createdBoPhases = new List<BoPhase>();

            try
            {
                foreach (var boPhaseDto in request.BoPhases)
                {
                    if (string.IsNullOrWhiteSpace(boPhaseDto.TraineeName))
                        return ApiResponse<List<BoPhaseDetailsDto>>.Fail("Trainee name cannot be empty");

                    if (string.IsNullOrWhiteSpace(boPhaseDto.Email))
                        return ApiResponse<List<BoPhaseDetailsDto>>.Fail("Trainee email cannot be empty");

                    if (string.IsNullOrWhiteSpace(boPhaseDto.BuddyName))
                        return ApiResponse<List<BoPhaseDetailsDto>>.Fail("Buddy name cannot be empty");

                    // Validate trainee by email
                    var user = await _userRepository.GetByEmailAsync(boPhaseDto.Email);
                    if (user == null)
                        return ApiResponse<List<BoPhaseDetailsDto>>.Fail($"Trainee with email {boPhaseDto.Email} not found");

                    // Verify trainee name matches
                    if (!user.Username.Equals(boPhaseDto.TraineeName, StringComparison.OrdinalIgnoreCase))
                        return ApiResponse<List<BoPhaseDetailsDto>>.Fail($"Trainee name {boPhaseDto.TraineeName} does not match email {boPhaseDto.Email}");

                    var trainee = await _traineeRepository.GetByUserIdAsync(user.Id);
                    if (trainee == null)
                        return ApiResponse<List<BoPhaseDetailsDto>>.Fail($"Trainee profile for {boPhaseDto.Email} not found");

                    // Check if trainee belongs to the specified batch
                    if (trainee.BatchId != request.BatchId)
                        return ApiResponse<List<BoPhaseDetailsDto>>.Fail($"Trainee with email {boPhaseDto.Email} does not belong to the specified batch");

                    // Check if trainee already has a BO Phase assignment (by trainee ID, not name)
                    var existingBoPhases = await _boPhaseRepository.GetByTraineeIdAsync(trainee.Id);
                    if (existingBoPhases.Any())
                        return ApiResponse<List<BoPhaseDetailsDto>>.Fail($"Trainee with email {boPhaseDto.Email} already has a BO Phase assignment");

                    // Get or create DU if provided
                    int? duId = null;
                    if (!string.IsNullOrWhiteSpace(boPhaseDto.DuName))
                    {
                        var du = await _duRepository.GetByNameAsync(boPhaseDto.DuName);
                        if (du == null)
                        {
                            du = new Du
                            {
                                Name = boPhaseDto.DuName,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };
                            du = await _duRepository.AddAsync(du);
                            if (du == null)
                                return ApiResponse<List<BoPhaseDetailsDto>>.Fail($"Failed to create DU with name {boPhaseDto.DuName}");
                        }
                        duId = du.Id;
                    }

                    // Get or create Buddy
                    var buddy = await _buddyRepository.GetByNameAsync(boPhaseDto.BuddyName);
                    if (buddy == null)
                    {
                        buddy = new Buddy
                        {
                            Name = boPhaseDto.BuddyName,
                            DuId = duId,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        buddy = await _buddyRepository.AddAsync(buddy);
                        if (buddy == null)
                            return ApiResponse<List<BoPhaseDetailsDto>>.Fail($"Failed to create Buddy with name {boPhaseDto.BuddyName}");
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
                        return ApiResponse<List<BoPhaseDetailsDto>>.Fail($"Failed to create BO Phase record for trainee with email {boPhaseDto.Email}");

                    createdBoPhases.Add(createdBoPhase);
                }

                // Get all created records with includes
                var boPhaseWithIncludes = await _boPhaseRepository.GetByIdsAsync(createdBoPhases.Select(b => b.Id));
                var boPhasesDtos = boPhaseWithIncludes.Select(bp => new BoPhaseDetailsDto
                {
                    BoPhaseId = bp.Id,
                    TraineeName = bp.Trainee?.User?.Username,
                    Buddy = bp.Buddy?.Name,
                    BuddyDU = bp.Buddy?.Du?.Name
                }).ToList();

                return ApiResponse<List<BoPhaseDetailsDto>>.Success(boPhasesDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<BoPhaseDetailsDto>>.Fail($"Error creating BO Phase records: {ex.Message}");
            }
        }
    }
}