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
    public class GetTraineesByBatchIdHandler : IRequestHandler<GetTraineesByBatchIdQuery, ApiResponse<List<TraineeDto>>>
    {
        private readonly ITraineeRepository _traineeRepository;
        private readonly IMapper _mapper;

        public GetTraineesByBatchIdHandler(ITraineeRepository traineeRepository, IMapper mapper)
        {
            _traineeRepository = traineeRepository;
            _mapper = mapper;
        }


        //public async Task<ApiResponse<List<TraineeDto>>> Handle(GetTraineesByBatchIdQuery request, CancellationToken cancellationToken)
        //{
        //    var trainees = await _traineeRepository.GetByBatchIdAsync(request.BatchId);
        //    var traineeDtos = _mapper.Map<List<TraineeDto>>(trainees);
        //}
        public async Task<ApiResponse<List<TraineeDto>>> Handle(
    GetTraineesByBatchIdQuery request,
    CancellationToken cancellationToken)
        {
            var trainees = await _traineeRepository.GetTraineesByBatchId(request.BatchId);

            var traineeDtos = _mapper.Map<List<TraineeDto>>(trainees);


            return ApiResponse<List<TraineeDto>>.Success(traineeDtos);
        }



    }
}
