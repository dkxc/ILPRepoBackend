
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Query.AdminDashboard;
using IlpRepoBackend.Domain.Persistence;
using System.Threading;


namespace IlpRepoBackend.Application.Handler.AdminDashboard
{
    public class GetAdminDashboardSummaryQueryHandler : IRequestHandler<GetAdminDashboardSummaryQuery, AdminDashboardSummaryDto>
    {
        private readonly IBatchRepository _batchRepo;
        private readonly IProjectRepository _projectRepo;

        public GetAdminDashboardSummaryQueryHandler(
            IBatchRepository batchRepo,
            IProjectRepository projectRepo)
        {
            _batchRepo = batchRepo;
            _projectRepo = projectRepo;
        }

        public async Task<AdminDashboardSummaryDto> Handle(GetAdminDashboardSummaryQuery request, CancellationToken cancellationToken)
        {
            var batches = await _batchRepo.GetAllAsync();
            var projects = await _projectRepo.GetAllAsync();

            return new AdminDashboardSummaryDto
            {
                TotalBatches = batches.Count(),
                TotalProjects = projects.Count()
            };
        }
    }
}




