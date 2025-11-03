using AutoMapper;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Query.Trainees;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handler.Trainees
{
    public class GetTraineeQueryHandler : IRequestHandler<GetTraineeQuery, ApiResponse<List<TraineeDto>>>
    {
        private readonly ITraineeRepository _traineeRepository;
        private readonly IMapper _mapper;

        public GetTraineeQueryHandler(ITraineeRepository traineeRepository, IMapper mapper)
        {
            _traineeRepository = traineeRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<List<TraineeDto>>> Handle(GetTraineeQuery request, CancellationToken cancellationToken)
        {
            var trainees = await _traineeRepository.GetAllAsync();
            if (trainees == null)
                return ApiResponse<List<TraineeDto>>.Fail("Failed to retrieve trainees");

            var traineeDtos = _mapper.Map<List<TraineeDto>>(trainees);
            return ApiResponse<List<TraineeDto>>.Success(traineeDtos);
        }
    }
}
