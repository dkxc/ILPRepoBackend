using IlpRepoBackend.Application.Dto.Dashboard;
using IlpRepoBackend.Application.Wrapper;
using MediatR;

namespace IlpRepoBackend.Application.Query.Dashboard
{
    public class GetTraineeDashboardQuery : IRequest<ApiResponse<TraineeDashboardDto>>
    {
        public int UserId { get; set; }
    }
}
