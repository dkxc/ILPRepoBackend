using AutoMapper;
using IlpRepoBackend.Application.Command.TraineeDus;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handler.TraineeDus
{
    public class UpdateTraineeDuHandler : IRequestHandler<UpdateTraineeDuCommand, ApiResponse<TraineeDuDetailsDto>>
    {
        private readonly ITraineeDuRepository _traineeDuRepository;
        private readonly ITraineeRepository _traineeRepository;
        private readonly IUserRepository _userRepository;
        private readonly IDuRepository _duRepository;
        private readonly IMapper _mapper;

        public UpdateTraineeDuHandler(
            ITraineeDuRepository traineeDuRepository,
            ITraineeRepository traineeRepository,
            IUserRepository userRepository,
            IDuRepository duRepository,
            IMapper mapper)
        {
            _traineeDuRepository = traineeDuRepository;
            _traineeRepository = traineeRepository;
            _userRepository = userRepository;
            _duRepository = duRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<TraineeDuDetailsDto>> Handle(UpdateTraineeDuCommand request, CancellationToken cancellationToken)
        {
            // Get existing TraineeDu
            var existingTraineeDu = await _traineeDuRepository.GetByIdAsync(request.TraineeDuId);
            if (existingTraineeDu == null)
                return ApiResponse<TraineeDuDetailsDto>.Fail($"TraineeDu assignment with ID {request.TraineeDuId} not found");

            // If trainee name is provided, update trainee
            if (!string.IsNullOrWhiteSpace(request.TraineeName))
            {
                var user = await _userRepository.GetByUsernameAsync(request.TraineeName);
                if (user == null)
                    return ApiResponse<TraineeDuDetailsDto>.Fail($"Trainee with name {request.TraineeName} not found");

                var trainee = await _traineeRepository.GetByUserIdAsync(user.Id);
                if (trainee == null)
                    return ApiResponse<TraineeDuDetailsDto>.Fail($"Trainee profile for {request.TraineeName} not found");

                // Check if this trainee already has a different DU assignment
                var existingTraineeDus = await _traineeDuRepository.GetByTraineeIdAsync(trainee.Id);
                if (existingTraineeDus.Any(td => td.Id != request.TraineeDuId))
                    return ApiResponse<TraineeDuDetailsDto>.Fail($"Trainee {request.TraineeName} already has a different DU assignment");

                existingTraineeDu.TraineeId = trainee.Id;
            }

            // Get or create DU
            var du = await _duRepository.GetByNameAsync(request.DuAllocated);
            if (du == null)
            {
                du = new Du
                {
                    Name = request.DuAllocated,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                du = await _duRepository.AddAsync(du);
                if (du == null)
                    return ApiResponse<TraineeDuDetailsDto>.Fail($"Failed to create DU with name {request.DuAllocated}");
            }

            // Update TraineeDu
            existingTraineeDu.DuId = du.Id;
            existingTraineeDu.Location = request.Location;
            existingTraineeDu.ojtMenter = request.OjtMentor;
            existingTraineeDu.UpdatedAt = DateTime.UtcNow;

            var updatedTraineeDu = await _traineeDuRepository.UpdateAsync(existingTraineeDu);
            
            // Get full record with includes for response
            var traineeDuWithIncludes = await _traineeDuRepository.GetByIdAsync(updatedTraineeDu.Id);
            var traineeDuDto = new TraineeDuDetailsDto
            {
                TraineeDuId = traineeDuWithIncludes.Id,
                TraineeName = traineeDuWithIncludes.Trainee?.User?.Username,
                DuAllocated = traineeDuWithIncludes.Du?.Name,
                Location = traineeDuWithIncludes.Location,
                OjtMentor = traineeDuWithIncludes.ojtMenter
            };

            return ApiResponse<TraineeDuDetailsDto>.Success(traineeDuDto);
        }
    }
}