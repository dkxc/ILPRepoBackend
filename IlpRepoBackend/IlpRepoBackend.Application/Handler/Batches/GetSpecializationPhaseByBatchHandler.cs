using AutoMapper;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Query.Batches;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handler.Batches
{
    public class GetSpecializationPhaseByBatchHandler : IRequestHandler<GetSpecializationPhaseByBatchQuery, ApiResponse<List<SpecializationPhaseDto>>>
    {
        private readonly ITraineeRepository _traineeRepository;
        private readonly IBatchRepository _batchRepository;

        public GetSpecializationPhaseByBatchHandler(
            ITraineeRepository traineeRepository,
            IBatchRepository batchRepository)
        {
            _traineeRepository = traineeRepository;
            _batchRepository = batchRepository;
        }

        public async Task<ApiResponse<List<SpecializationPhaseDto>>> Handle(GetSpecializationPhaseByBatchQuery request, CancellationToken cancellationToken)
        {
            // Validate batch exists
            var batch = await _batchRepository.GetByIdAsync(request.BatchId);
            if (batch == null)
                return ApiResponse<List<SpecializationPhaseDto>>.Fail($"Batch with ID {request.BatchId} not found");

            // Get trainees with their project assignments
            var trainees = await _traineeRepository.GetTraineesWithProjectsByBatchIdAsync(request.BatchId);

            if (!trainees.Any())
                return ApiResponse<List<SpecializationPhaseDto>>.Success(new List<SpecializationPhaseDto>());

            var specializationPhaseData = new List<SpecializationPhaseDto>();

            foreach (var trainee in trainees)
            {
                // Get the most recent or primary project assignment
                var projectTeam = trainee.ProjectTeams?
                    .OrderByDescending(pt => pt.CreatedAt)
                    .FirstOrDefault();

                var dto = new SpecializationPhaseDto
                {
                    Id = trainee.Id,
                    TraineeId = trainee.Id,
                    TraineeName = trainee.User?.Username ?? "Unknown",
                    TechStack = projectTeam?.Project?.Technology ?? "Not Assigned",
                    Project = projectTeam?.Project?.ProjectName ?? "Not Assigned",
                    SpecializationPhaseId = projectTeam?.Id
                };

                specializationPhaseData.Add(dto);
            }

            return ApiResponse<List<SpecializationPhaseDto>>.Success(specializationPhaseData);
        }
    }
}