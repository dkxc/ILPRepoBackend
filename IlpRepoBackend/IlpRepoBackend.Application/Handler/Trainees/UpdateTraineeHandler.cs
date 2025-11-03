using AutoMapper;
using IlpRepoBackend.Application.Command.Trainees;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handler.Trainees
{
    public class UpdateTraineeHandler : IRequestHandler<UpdateTraineeCommand, ApiResponse<TraineeDto>>
    {
        private readonly ITraineeRepository _traineeRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UpdateTraineeHandler(
            ITraineeRepository traineeRepository,
            IUserRepository userRepository,
            IMapper mapper)
        {
            _traineeRepository = traineeRepository ?? throw new ArgumentNullException(nameof(traineeRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ApiResponse<TraineeDto>> Handle(UpdateTraineeCommand request, CancellationToken cancellationToken)
        {
            var trainee = await _traineeRepository.GetByIdAsync(request.Id);
            if (trainee == null)
                return ApiResponse<TraineeDto>.Fail($"Trainee with ID {request.Id} not found");

            // Get the current user to update User entity fields
            var currentUser = await _userRepository.GetByIdAsync(trainee.UserId);
            if (currentUser == null)
                return ApiResponse<TraineeDto>.Fail($"User account for trainee not found");

            // Update username if provided
            if (!string.IsNullOrWhiteSpace(request.Username) && request.Username != currentUser.Username)
            {
                currentUser.Username = request.Username;
                currentUser.UpdatedAt = DateTime.UtcNow;
            }

            // Check if email is being updated and if it's not already in use
            if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != trainee.Email)
            {
                if (request.Email != currentUser.Email && await _userRepository.EmailExistsAsync(request.Email))
                {
                    return ApiResponse<TraineeDto>.Fail("Email already exists");
                }
                
                // Update email in User entity
                currentUser.Email = request.Email;
                currentUser.UpdatedAt = DateTime.UtcNow;
            }

            // Save user updates if any changes were made
            if (!string.IsNullOrWhiteSpace(request.Username) || 
                (!string.IsNullOrWhiteSpace(request.Email) && request.Email != trainee.Email))
            {
                await _userRepository.UpdateAsync(currentUser);
            }

            // Update trainee properties
            trainee.Email = request.Email ?? trainee.Email;
            trainee.PhoneNo = request.PhoneNo ?? trainee.PhoneNo;
            trainee.Status = request.Status;
            trainee.BloodGroup = request.BloodGroup ?? trainee.BloodGroup;
            trainee.AadhaarId = request.AadhaarId ?? trainee.AadhaarId;
            trainee.HealthCondition = request.HealthCondition ?? trainee.HealthCondition;
            trainee.PersonalInterest = request.PersonalInterest ?? trainee.PersonalInterest;
            trainee.Address = request.Address ?? trainee.Address;
            trainee.CurrentAddress = request.CurrentAddress ?? trainee.CurrentAddress;
            trainee.ContactNumber = request.ContactNumber ?? trainee.ContactNumber;
            trainee.EmergencyContactName = request.EmergencyContactName ?? trainee.EmergencyContactName;
            trainee.EmergencyContactRelationship = request.EmergencyContactRelationship ?? trainee.EmergencyContactRelationship;
            trainee.EmergencyContactNo = request.EmergencyContactNo ?? trainee.EmergencyContactNo;
            trainee.UpdatedAt = DateTime.UtcNow;

            var updatedTrainee = await _traineeRepository.UpdateAsync(trainee);
            var traineeDto = _mapper.Map<TraineeDto>(updatedTrainee);
            traineeDto.Username = currentUser.Username;  // Ensure updated username is returned
            
            return ApiResponse<TraineeDto>.Success(traineeDto);
        }
    }
}