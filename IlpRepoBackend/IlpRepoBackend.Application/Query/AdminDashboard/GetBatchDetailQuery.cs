using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IlpRepoBackend.Application.Dto;
using MediatR;

namespace IlpRepoBackend.Application.Query.AdminDashboard
{
    public class GetBatchDetailQuery : IRequest<BatchDetailDto>
    {
        public int BatchId { get; }

        public GetBatchDetailQuery(int batchId)
        {
            BatchId = batchId;
        }
    }
}
