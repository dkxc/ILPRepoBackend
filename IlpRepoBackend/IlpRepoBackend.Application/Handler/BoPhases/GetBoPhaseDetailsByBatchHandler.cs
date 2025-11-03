using AutoMapper;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Query.BoPhases;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handler.BoPhases
{
    public class GetBoPhaseDetailsByBatchHandler : IRequestHandler<GetBoPhaseDetailsByBatchQuery, ApiResponse<List<BoPhaseDetailsDto>>>
    {
        private readonly IBoPhaseRepository _boPhaseRepository;
        private readonly ITraineeRepository _traineeRepository;
        private readonly IMapper _mapper;

        public GetBoPhaseDetailsByBatchHandler(
            IBoPhaseRepository boPhaseRepository,
            ITraineeRepository traineeRepository,
            IMapper mapper)
        {
            _boPhaseRepository = boPhaseRepository;
            _traineeRepository = traineeRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<List<BoPhaseDetailsDto>>> Handle(
            GetBoPhaseDetailsByBatchQuery request,
            CancellationToken cancellationToken)
        {
            // Get trainees for the batch
            var trainees = await _traineeRepository.GetByBatchIdAsync(request.BatchId);
            if (!trainees.Any())
            {
                return ApiResponse<List<BoPhaseDetailsDto>>.Fail("No trainees found for the specified batch");
            }

            var traineeIds = trainees.Select(t => t.Id).ToList();
            
            // Get BO Phase details with includes
            var boPhaseDetails = await _boPhaseRepository.GetBoPhaseDetailsByTraineeIds(traineeIds);
            
            if (!boPhaseDetails.Any())
            {
                return ApiResponse<List<BoPhaseDetailsDto>>.Fail("No BO Phase assignments found for the trainees in this batch");
            }

            var detailsDtos = boPhaseDetails.Select(bp => new BoPhaseDetailsDto
            {
                BoPhaseId = bp.Id,
                TraineeName = bp.Trainee?.User?.Username,
                Buddy = bp.Buddy?.Name,
                BuddyDU = bp.Buddy?.Du?.Name
            }).ToList();

            return ApiResponse<List<BoPhaseDetailsDto>>.Success(detailsDtos);
        }
    }
}