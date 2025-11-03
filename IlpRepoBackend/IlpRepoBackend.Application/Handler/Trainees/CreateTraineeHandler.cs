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
using System.Threading;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handler.Trainees
{
    public class CreateTraineeHandler : IRequestHandler<CreateTraineeCommand, ApiResponse<TraineeDto>>
    {
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        private readonly ITraineeRepository _traineeRepository;

        public CreateTraineeHandler(IMapper mapper, IUserRepository userRepository, ITraineeRepository traineeRepository)
        {
            _mapper = mapper;
            _userRepository = userRepository;
            _traineeRepository = traineeRepository;
        }

        public async Task<ApiResponse<TraineeDto>> Handle(CreateTraineeCommand request, CancellationToken cancellationToken)
        {
            // Check if email already exists
            if (await _userRepository.EmailExistsAsync(request.Email))
                return ApiResponse<TraineeDto>.Fail("Email already exists");

            // Check if AadhaarId already exists
            if (!string.IsNullOrEmpty(request.AadhaarId))
            {
                var aadhaarExists = await _traineeRepository.AadhaarIdExistsAsync(request.AadhaarId);
                if (aadhaarExists)
                    return ApiResponse<TraineeDto>.Fail("A trainee with this Aadhaar ID already exists.");
            }

            // Create User
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
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
                Email = request.Email,
                BatchId = request.BatchId,
                PhoneNo = request.PhoneNo,
                Status = request.Status,
                BloodGroup = request.BloodGroup,
                AadhaarId = request.AadhaarId,
                HealthCondition = request.HealthCondition,
                PersonalInterest = request.PersonalInterest,
                Address = request.Address,
                CurrentAddress = request.CurrentAddress,
                ContactNumber = request.ContactNumber,
                EmergencyContactName = request.EmergencyContactName,
                EmergencyContactRelationship = request.EmergencyContactRelationship,
                EmergencyContactNo = request.EmergencyContactNo,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdTrainee = await _traineeRepository.AddAsync(trainee);
            var traineeDto = _mapper.Map<TraineeDto>(createdTrainee);
            traineeDto.Username = createdUser.Username;

            return ApiResponse<TraineeDto>.Success(traineeDto);
        }
    }
}
