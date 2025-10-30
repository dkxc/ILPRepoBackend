using IlpRepoBackend.Application.Dto.Project;
using IlpRepoBackend.Application.Wrapper;
using MediatR;
using System.Collections.Generic;

namespace IlpRepoBackend.Application.Query.Projects
{
    public class GetAllProjectsQuery : IRequest<ApiResponse<List<ProjectDto>>>
    {
        public int? BatchId { get; set; } // Optional: filter by batch
        public string? Status { get; set; } // Optional: filter by status
        public string? Technology { get; set; } // Optional: filter by technology

        public GetAllProjectsQuery()
        {
        }

        public GetAllProjectsQuery(int? batchId = null, string? status = null, string? technology = null)
        {
            BatchId = batchId;
            Status = status;
            Technology = technology;
        }
    }
}