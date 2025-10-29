using AutoMapper;
using IlpRepoBackend.Application.Command.Trainees;
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

namespace IlpRepoBackend.Application.Handler.Trainees
{
    public class CreateTraineeByBatchHandler
        : IRequestHandler<CreateTraineeByBatch, ApiResponse<List<TraineeDto>>>
    {
        private readonly ITraineeRepository _traineeRepository;
        private readonly IUserRepository _userRepository;
        private readonly IBatchRepository _batchRepository;
        private readonly IMapper _mapper;

        public CreateTraineeByBatchHandler(
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

        public async Task<ApiResponse<List<TraineeDto>>> Handle(
            CreateTraineeByBatch request,
            CancellationToken cancellationToken)
        {
            // Validate input
            if (request.Trainees == null || request.Trainees.Count == 0)
                return ApiResponse<List<TraineeDto>>.Fail("No trainees to add");

            // Validate batch exists
            var batch = await _batchRepository.GetByIdAsync(request.BatchId);
            if (batch == null)
                return ApiResponse<List<TraineeDto>>.Fail("Batch does not exist");

            // Validate all emails upfront
            var emails = request.Trainees.Select(t => t.Email).Where(e => !string.IsNullOrEmpty(e)).ToList();
            foreach (var email in emails)
            {
                if (await _userRepository.EmailExistsAsync(email))
                    return ApiResponse<List<TraineeDto>>.Fail($"Email {email} already exists");
            }

            // Check for duplicate emails in the request itself
            var duplicateEmails = emails.GroupBy(e => e).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (duplicateEmails.Any())
                return ApiResponse<List<TraineeDto>>.Fail($"Duplicate emails in request: {string.Join(", ", duplicateEmails)}");

            // Validate passwords
            foreach (var traineeDto in request.Trainees)
            {
                if (string.IsNullOrWhiteSpace(traineeDto.Password))
                    return ApiResponse<List<TraineeDto>>.Fail($"Password is required for {traineeDto.Username}");

                if (traineeDto.Password.Length < 6)
                    return ApiResponse<List<TraineeDto>>.Fail($"Password must be at least 6 characters for {traineeDto.Username}");
            }

            var createdTrainees = new List<Trainee>();

            // TODO: Wrap this in a database transaction for atomicity
            // using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                foreach (var traineeDto in request.Trainees)
                {
                    // Create user
                    var user = new User
                    {
                        Username = traineeDto.Username,
                        Email = traineeDto.Email,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(traineeDto.Password),
                        Role = Domain.Enum.UserRole.Trainee,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    var createdUser = await _userRepository.AddAsync(user);
                    if (createdUser == null)
                        return ApiResponse<List<TraineeDto>>.Fail($"Failed to create user {traineeDto.Username}");

                    // Create trainee
                    var trainee = _mapper.Map<Trainee>(traineeDto);
                    trainee.UserId = createdUser.Id;
                    trainee.BatchId = request.BatchId;
                    trainee.CreatedAt = DateTime.UtcNow;
                    trainee.UpdatedAt = DateTime.UtcNow;

                    var createdTrainee = await _traineeRepository.AddAsync(trainee);
                    if (createdTrainee == null)
                        return ApiResponse<List<TraineeDto>>.Fail($"Failed to create trainee for {traineeDto.Username}");

                    createdTrainees.Add(createdTrainee);
                }

                // await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                // await transaction.RollbackAsync(cancellationToken);
                return ApiResponse<List<TraineeDto>>.Fail($"Error creating trainees: {ex.Message}");
            }

            // Map and return
            var traineeDtos = _mapper.Map<List<TraineeDto>>(createdTrainees);
            return ApiResponse<List<TraineeDto>>.Success(traineeDtos);
        }
    }
}