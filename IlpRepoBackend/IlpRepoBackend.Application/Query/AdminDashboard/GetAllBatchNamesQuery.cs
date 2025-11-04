using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IlpRepoBackend.Application.Dto;
using MediatR;

namespace IlpRepoBackend.Application.Query.AdminDashboard
{
    public class GetAllBatchNamesQuery : IRequest<List<BatchNameDto>>
    {
    }
}
