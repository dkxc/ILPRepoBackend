using IlpRepoBackend.Application.Dto.Project;
using IlpRepoBackend.Application.Wrapper;
using MediatR;
using System.Collections.Generic;

namespace IlpRepoBackend.Application.Command.Projects
{
    public class CreateBatchProjectsCommand : IRequest<ApiResponse<List<ProjectDto>>>
    {
        public CreateBatchProjectsDto BatchProjectsData { get; set; }

        public CreateBatchProjectsCommand(CreateBatchProjectsDto batchProjectsData)
        {
            BatchProjectsData = batchProjectsData;
        }
    }
}