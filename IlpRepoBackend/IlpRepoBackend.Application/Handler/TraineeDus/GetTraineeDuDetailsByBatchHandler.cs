using AutoMapper;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Query.TraineeDus;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handler.TraineeDus
{
    public class GetTraineeDuDetailsByBatchHandler : IRequestHandler<GetTraineeDuDetailsByBatchQuery, ApiResponse<List<TraineeDuDetailsDto>>>
    {
        private readonly ITraineeDuRepository _traineeDuRepository;
        private readonly ITraineeRepository _traineeRepository;
        private readonly IMapper _mapper;

        public GetTraineeDuDetailsByBatchHandler(
            ITraineeDuRepository traineeDuRepository,
            ITraineeRepository traineeRepository,
            IMapper mapper)
        {
            _traineeDuRepository = traineeDuRepository;
            _traineeRepository = traineeRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<List<TraineeDuDetailsDto>>> Handle(
            GetTraineeDuDetailsByBatchQuery request,
            CancellationToken cancellationToken)
        {
            // Get trainees for the batch
            var trainees = await _traineeRepository.GetByBatchIdAsync(request.BatchId);
            if (!trainees.Any())
            {
                return ApiResponse<List<TraineeDuDetailsDto>>.Fail("No trainees found for the specified batch");
            }

            var traineeIds = trainees.Select(t => t.Id).ToList();
            
            // Get TraineeDu details with Du information
            var traineeDuDetails = await _traineeDuRepository.GetTraineeDuDetailsByTraineeIds(traineeIds);
            
            if (!traineeDuDetails.Any())
            {
                return ApiResponse<List<TraineeDuDetailsDto>>.Fail("No DU assignments found for the trainees in this batch");
            }

            var detailsDtos = traineeDuDetails.Select(td => new TraineeDuDetailsDto
            {
                TraineeDuId = td.Id,
                TraineeName = td.Trainee?.User?.Username,
                DuAllocated = td.Du?.Name,
                Location = td.Location,
                OjtMentor = td.ojtMenter
            }).ToList();

            return ApiResponse<List<TraineeDuDetailsDto>>.Success(detailsDtos);
        }
    }
}