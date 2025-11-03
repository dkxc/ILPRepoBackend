using AutoMapper;
using BCrypt.Net;
using IlpRepoBackend.Application.Command.Trainees;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Enum;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handler.Trainees
{
    public class CreateTraineeByBatchHandler : IRequestHandler<CreateTraineeByBatch, ApiResponse<List<TraineeDto>>>
    {
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        private readonly ITraineeRepository _traineeRepository;
        private readonly IBatchRepository _batchRepository;

        public CreateTraineeByBatchHandler(
            IMapper mapper,
            IUserRepository userRepository,
            ITraineeRepository traineeRepository,
            IBatchRepository batchRepository)
        {
            _mapper = mapper;
            _userRepository = userRepository;
            _traineeRepository = traineeRepository;
            _batchRepository = batchRepository;
        }

        public async Task<ApiResponse<List<TraineeDto>>> Handle(CreateTraineeByBatch request, CancellationToken cancellationToken)
        {
            if (request?.Trainees == null || !request.Trainees.Any())
            {
                return ApiResponse<List<TraineeDto>>.Fail("No trainees to add");
            }

            // Validate batch exists
            var batch = await _batchRepository.GetByIdAsync(request.BatchId);
            if (batch == null)
            {
                return ApiResponse<List<TraineeDto>>.Fail("Batch does not exist");
            }

            var createdTraineeDtos = new List<TraineeDto>();

            try
            {
                foreach (var traineeDto in request.Trainees)
                {
                    // Check if email already exists
                    var emailExists = await _userRepository.EmailExistsAsync(traineeDto.Email);
                    if (emailExists)
                    {
                        return ApiResponse<List<TraineeDto>>.Fail($"Email {traineeDto.Email} already exists");
                    }

                    // Check if AadhaarId already exists
                    if (!string.IsNullOrEmpty(traineeDto.AadhaarId))
                    {
                        var aadhaarExists = await _traineeRepository.AadhaarIdExistsAsync(traineeDto.AadhaarId);
                        if (aadhaarExists)
                        {
                            return ApiResponse<List<TraineeDto>>.Fail($"Aadhaar ID {traineeDto.AadhaarId} already exists");
                        }
                    }

                    // Create User
                    var user = new User
                    {
                        Username = traineeDto.Username,
                        Email = traineeDto.Email ?? string.Empty,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(traineeDto.Password),
                        Role = UserRole.Trainee,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    var createdUser = await _userRepository.AddAsync(user);

                    // Create Trainee
                    var trainee = new Trainee
                    {
                        UserId = createdUser.Id,
                        Email = traineeDto.Email,
                        BatchId = request.BatchId,
                        PhoneNo = traineeDto.PhoneNo,
                        Status = traineeDto.Status,
                        BloodGroup = traineeDto.BloodGroup,
                        AadhaarId = traineeDto.AadhaarId,
                        HealthCondition = traineeDto.HealthCondition,
                        PersonalInterest = traineeDto.PersonalInterest,
                        Address = traineeDto.Address,
                        CurrentAddress = traineeDto.CurrentAddress,
                        ContactNumber = traineeDto.ContactNumber,
                        EmergencyContactName = traineeDto.EmergencyContactName,
                        EmergencyContactRelationship = traineeDto.EmergencyContactRelationship,
                        EmergencyContactNo = traineeDto.EmergencyContactNo,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    var createdTrainee = await _traineeRepository.AddAsync(trainee);
                    var resultDto = _mapper.Map<TraineeDto>(createdTrainee);
                    resultDto.Username = createdUser.Username;
                    resultDto.BatchName = batch.BatchName;

                    createdTraineeDtos.Add(resultDto);
                }

                return ApiResponse<List<TraineeDto>>.Success(createdTraineeDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<TraineeDto>>.Fail($"Error creating trainees: {ex.Message}");
            }
        }
    }
}