using IlpRepoBackend.Application.Dto.Project;
using IlpRepoBackend.Application.Wrapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Command.Projects
{
    public class UpdateProjectCommand : IRequest<ApiResponse<ProjectDto>>
    {
        public int ProjectId { get; set; }
        public CreateProjectDto ProjectData { get; set; } = null!;

        public UpdateProjectCommand(int projectId, CreateProjectDto projectData)
        {
            ProjectId = projectId;
            ProjectData = projectData;
        }
    }
}
