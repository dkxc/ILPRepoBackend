using AutoMapper;
using IlpRepoBackend.Application.Command.Trainees;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handler.Trainees
{
    public class CreateTraineeHandler : IRequestHandler<CreateTraineeCommand, ApiResponse<TraineeDto>>
    {
        private readonly ITraineeRepository _traineeRepository;
        private readonly IUserRepository _userRepository;
        private readonly IBatchRepository _batchRepository;
        private readonly IMapper _mapper;

        public CreateTraineeHandler(
            ITraineeRepository traineeRepository,
            IUserRepository userRepository,
            IBatchRepository batchRepository,
            IMapper mapper)
        {
            _traineeRepository = traineeRepository;
            _userRepository = userRepository;
            _batchRepository = batchRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<TraineeDto>> Handle(CreateTraineeCommand request, CancellationToken cancellationToken)
        {
            // 1. Validate email
            if (await _userRepository.EmailExistsAsync(request.Email))
                return ApiResponse<TraineeDto>.Fail("Email already exists");

            // 2. Validate batch
            var batch = await _batchRepository.GetByIdAsync(request.BatchId);
            if (batch == null)
                return ApiResponse<TraineeDto>.Fail("Batch does not exist");

            // 3. Create user
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = Domain.Enum.UserRole.Trainee,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdUser = await _userRepository.AddAsync(user);
            if (createdUser == null)
                return ApiResponse<TraineeDto>.Fail("Failed to create user");

            // 4. Create trainee
            var trainee = _mapper.Map<Trainee>(request);
            trainee.UserId = createdUser.Id;
            var createdTrainee = await _traineeRepository.AddAsync(trainee);

            if (createdTrainee == null)
                return ApiResponse<TraineeDto>.Fail("Failed to create trainee");

            // 5. Fetch trainee with related data (includes)
            var traineeWithIncludes = await _traineeRepository.GetByIdAsync(createdTrainee.Id);
            var traineeDto = _mapper.Map<TraineeDto>(traineeWithIncludes);

            // 6. Return success
            return ApiResponse<TraineeDto>.Success(traineeDto);
        }
    }
}
