using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Query.Trainees
{
    public class GetTraineesByBatchIdQuery : IRequest<ApiResponse<List<TraineeDto>>>
    {
        public GetTraineesByBatchIdQuery(int batchId)
        {
            BatchId = batchId;
        }

        public int BatchId { get;  set; }
    }
}
