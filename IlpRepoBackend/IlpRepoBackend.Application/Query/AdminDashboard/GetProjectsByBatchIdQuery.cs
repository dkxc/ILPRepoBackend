using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IlpRepoBackend.Application.Dto;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Query.AdminDashboard
{
    public class GetProjectsByBatchIdQuery : IRequest<List<ProjectsByBatchIdDto>>
    {
        public int BatchId { get; set; }

        public GetProjectsByBatchIdQuery(int batchId)
        {
            BatchId = batchId;
        }
    }
}
