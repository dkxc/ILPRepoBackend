using AutoMapper;
using IlpRepoBackend.Application.Command.TraineeDus;
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

namespace IlpRepoBackend.Application.Handler.TraineeDus
{
    public class CreateTraineeDuByBatchHandler : IRequestHandler<CreateTraineeDuByBatchCommand, ApiResponse<List<TraineeDuDto>>>
    {
        private readonly ITraineeDuRepository _traineeDuRepository;
        private readonly ITraineeRepository _traineeRepository;
        private readonly IDuRepository _duRepository;
        private readonly IUserRepository _userRepository;
        private readonly IBatchRepository _batchRepository;
        private readonly IMapper _mapper;

        public CreateTraineeDuByBatchHandler(
            ITraineeDuRepository traineeDuRepository,
            ITraineeRepository traineeRepository,
            IDuRepository duRepository,
            IUserRepository userRepository,
            IBatchRepository batchRepository,
            IMapper mapper)
        {
            _traineeDuRepository = traineeDuRepository ?? throw new ArgumentNullException(nameof(traineeDuRepository));
            _traineeRepository = traineeRepository ?? throw new ArgumentNullException(nameof(traineeRepository));
            _duRepository = duRepository ?? throw new ArgumentNullException(nameof(duRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _batchRepository = batchRepository ?? throw new ArgumentNullException(nameof(batchRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ApiResponse<List<TraineeDuDto>>> Handle(CreateTraineeDuByBatchCommand request, CancellationToken cancellationToken)
        {
            if (request?.TraineeDus == null || !request.TraineeDus.Any())
                return ApiResponse<List<TraineeDuDto>>.Fail("No TraineeDu records to add");

            // Validate batch exists
            var batch = await _batchRepository.GetByIdAsync(request.BatchId);
            if (batch == null)
                return ApiResponse<List<TraineeDuDto>>.Fail("Batch does not exist");

            var createdTraineeDus = new List<TraineeDu>();

            try
            {
                foreach (var traineeDuDto in request.TraineeDus)
                {
                    if (string.IsNullOrWhiteSpace(traineeDuDto.TraineeName))
                        return ApiResponse<List<TraineeDuDto>>.Fail("Trainee name cannot be empty");

                    if (string.IsNullOrWhiteSpace(traineeDuDto.Email))
                        return ApiResponse<List<TraineeDuDto>>.Fail("Trainee email cannot be empty");

                    if (string.IsNullOrWhiteSpace(traineeDuDto.DuName))
                        return ApiResponse<List<TraineeDuDto>>.Fail("DU name cannot be empty");

                    // Validate trainee by email
                    var user = await _userRepository.GetByEmailAsync(traineeDuDto.Email);
                    if (user == null)
                        return ApiResponse<List<TraineeDuDto>>.Fail($"Trainee with email {traineeDuDto.Email} not found");

                    // Verify trainee name matches
                    if (!user.Username.Equals(traineeDuDto.TraineeName, StringComparison.OrdinalIgnoreCase))
                        return ApiResponse<List<TraineeDuDto>>.Fail($"Trainee name {traineeDuDto.TraineeName} does not match email {traineeDuDto.Email}");

                    var trainee = await _traineeRepository.GetByUserIdAsync(user.Id);
                    if (trainee == null)
                        return ApiResponse<List<TraineeDuDto>>.Fail($"Trainee profile for {traineeDuDto.Email} not found");

                    // Check if trainee belongs to the specified batch
                    if (trainee.BatchId != request.BatchId)
                        return ApiResponse<List<TraineeDuDto>>.Fail($"Trainee {traineeDuDto.TraineeName} does not belong to the specified batch");

                    // Check if trainee already has a DU assignment
                    var existingTraineeDu = await _traineeDuRepository.GetByTraineeIdAsync(trainee.Id);
                    if (existingTraineeDu.Any())
                        return ApiResponse<List<TraineeDuDto>>.Fail($"Trainee {traineeDuDto.TraineeName} already has a DU assignment");

                    // Get or create DU
                    var du = await _duRepository.GetByNameAsync(traineeDuDto.DuName);
                    if (du == null)
                    {
                        du = new Du
                        {
                            Name = traineeDuDto.DuName,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        du = await _duRepository.AddAsync(du);
                        if (du == null)
                            return ApiResponse<List<TraineeDuDto>>.Fail($"Failed to create DU with name {traineeDuDto.DuName}");
                    }

                    // Create TraineeDu
                    var traineeDu = new TraineeDu
                    {
                        TraineeId = trainee.Id,
                        DuId = du.Id,
                        Location = traineeDuDto.Location ?? string.Empty,
                        ojtMenter = traineeDuDto.OjtMenter ?? string.Empty,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    var createdTraineeDu = await _traineeDuRepository.AddAsync(traineeDu);
                    if (createdTraineeDu == null)
                        return ApiResponse<List<TraineeDuDto>>.Fail($"Failed to create TraineeDu record for trainee {traineeDuDto.TraineeName}");

                    createdTraineeDus.Add(createdTraineeDu);
                }

                // Get all created records with includes
                var traineeDusWithIncludes = await _traineeDuRepository.GetByIdsAsync(createdTraineeDus.Select(t => t.Id));
                var traineeDuDtos = _mapper.Map<List<TraineeDuDto>>(traineeDusWithIncludes);

                return ApiResponse<List<TraineeDuDto>>.Success(traineeDuDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<TraineeDuDto>>.Fail($"Error creating TraineeDu records: {ex.Message}");
            }
        }
    }
}