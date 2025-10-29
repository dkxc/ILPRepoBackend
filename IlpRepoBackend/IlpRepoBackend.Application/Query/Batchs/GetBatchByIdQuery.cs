using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Query.Batchs
{
    public class GetBatchByIdQuery : IRequest<ApiResponse<BatchDto>>
    {
        public int Id { get; set; }
    }
}
