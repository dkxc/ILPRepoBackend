using MediatR;
using IlpRepoBackend.Application.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Query.AdminDashboard
{
    public class GetAdminDashboardSummaryQuery : IRequest<AdminDashboardSummaryDto>
    {
    }
}

