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
    public class CreateTraineeDuHandler : IRequestHandler<CreateTraineeDuCommand, ApiResponse<TraineeDuDto>>
    {
        private readonly ITraineeDuRepository _traineeDuRepository;
        private readonly ITraineeRepository _traineeRepository;
        private readonly IDuRepository _duRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public CreateTraineeDuHandler(
            ITraineeDuRepository traineeDuRepository,
            ITraineeRepository traineeRepository,
            IDuRepository duRepository,
            IUserRepository userRepository,
            IMapper mapper)
        {
            _traineeDuRepository = traineeDuRepository ?? throw new ArgumentNullException(nameof(traineeDuRepository));
            _traineeRepository = traineeRepository ?? throw new ArgumentNullException(nameof(traineeRepository));
            _duRepository = duRepository ?? throw new ArgumentNullException(nameof(duRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ApiResponse<TraineeDuDto>> Handle(CreateTraineeDuCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.TraineeName))
                return ApiResponse<TraineeDuDto>.Fail("Trainee name cannot be empty");

            if (string.IsNullOrWhiteSpace(request.Email))
                return ApiResponse<TraineeDuDto>.Fail("Trainee email cannot be empty");

            if (string.IsNullOrWhiteSpace(request.DuName))
                return ApiResponse<TraineeDuDto>.Fail("DU name cannot be empty");

            // Validate trainee by email
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
                return ApiResponse<TraineeDuDto>.Fail($"Trainee with email {request.Email} not found");

            // Verify trainee name matches
            if (!user.Username.Equals(request.TraineeName, StringComparison.OrdinalIgnoreCase))
                return ApiResponse<TraineeDuDto>.Fail($"Trainee name {request.TraineeName} does not match email {request.Email}");

            var trainee = await _traineeRepository.GetByUserIdAsync(user.Id);
            if (trainee == null)
                return ApiResponse<TraineeDuDto>.Fail($"Trainee profile for {request.Email} not found");

            // Check if trainee already has a DU assignment
            var existingTraineeDu = await _traineeDuRepository.GetByTraineeIdAsync(trainee.Id);
            if (existingTraineeDu.Any())
                return ApiResponse<TraineeDuDto>.Fail($"Trainee {request.TraineeName} already has a DU assignment");

            // Get or create DU
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
                    return ApiResponse<TraineeDuDto>.Fail($"Failed to create DU with name {request.DuName}");
            }

            // Create TraineeDu
            var traineeDu = new TraineeDu
            {
                TraineeId = trainee.Id,
                DuId = du.Id,
                Location = request.Location,
                ojtMenter = request.OjtMenter,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdTraineeDu = await _traineeDuRepository.AddAsync(traineeDu);
            if (createdTraineeDu == null)
                return ApiResponse<TraineeDuDto>.Fail("Failed to create TraineeDu record");

            // Get full record with includes
            var traineeDuWithIncludes = await _traineeDuRepository.GetByIdAsync(createdTraineeDu.Id);
            var traineeDuDto = _mapper.Map<TraineeDuDto>(traineeDuWithIncludes);
            
            return ApiResponse<TraineeDuDto>.Success(traineeDuDto);
        }
    }
}